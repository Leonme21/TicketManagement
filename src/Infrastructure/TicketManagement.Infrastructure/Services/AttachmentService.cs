using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TicketManagement.Application.Common.Interfaces;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Entities;
using TicketManagement.Domain.Interfaces;
using System.Diagnostics;

namespace TicketManagement.Infrastructure.Services;

/// <summary>
///  Service implementation for secure attachment handling
/// </summary>
public class AttachmentService : IAttachmentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<AttachmentService> _logger;
    private readonly AttachmentSettings _settings;
    private static readonly ActivitySource _activitySource = new("TicketManagement.Infrastructure");

    //  MIME type whitelist based on extensions
    private static readonly Dictionary<string, List<string>> AllowedExtensions = new()
    {
        { ".jpg", new List<string> { "image/jpeg", "image/pjpeg" } },
        { ".jpeg", new List<string> { "image/jpeg", "image/pjpeg" } },
        { ".png", new List<string> { "image/png" } },
        { ".gif", new List<string> { "image/gif" } },
        { ".pdf", new List<string> { "application/pdf" } },
        { ".doc", new List<string> { "application/msword" } },
        { ".docx", new List<string> { "application/vnd.openxmlformats-officedocument.wordprocessingml.document" } },
        { ".xls", new List<string> { "application/vnd.ms-excel" } },
        { ".xlsx", new List<string> { "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" } },
        { ".txt", new List<string> { "text/plain" } },
        { ".zip", new List<string> { "application/zip", "application/x-zip-compressed" } }
    };

    //  Magic bytes (File Signatures) for deep validation
    private static readonly Dictionary<string, List<byte[]>> MagicBytes = new()
    {
        { ".jpg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".jpeg", new List<byte[]> { new byte[] { 0xFF, 0xD8, 0xFF } } },
        { ".png", new List<byte[]> { new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A } } },
        { ".gif", new List<byte[]> { new byte[] { 0x47, 0x49, 0x46, 0x38 } } },
        { ".pdf", new List<byte[]> { new byte[] { 0x25, 0x50, 0x44, 0x46 } } },
        { ".zip", new List<byte[]> { new byte[] { 0x50, 0x4B, 0x03, 0x04 } } }
    };

    private static readonly Action<ILogger, int, string, Exception?> _logFileUploadSuccess =
        LoggerMessage.Define<int, string>(
            LogLevel.Information,
            new EventId(5001, "FileUploadSuccess"),
            "Attachment {AttachmentId} ({FileName}) uploaded correctly");

    public AttachmentService(
        IUnitOfWork unitOfWork,
        ILogger<AttachmentService> logger,
        IOptions<AttachmentSettings> settings)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task<Result<int>> UploadAttachmentAsync(
        FileUploadRequest file, 
        int ticketId, 
        int userId, 
        CancellationToken cancellationToken = default)
    {
        using var activity = _activitySource.StartActivity("UploadAttachment");
        activity?.SetTag("file.name", file.FileName);
        activity?.SetTag("file.size", file.Length);

        try
        {
            // 1.  Validate file
            var validationResult = await ValidateFileAsync(file, cancellationToken);
            if (!validationResult.IsSuccess || !validationResult.Value!.IsValid)
            {
                var error = validationResult.Error.Description ?? validationResult.Value?.ErrorMessage ?? "Invalid file";
                _logger.LogWarning("File validation failed for {FileName}: {Error}", file.FileName, error);
                return Result<int>.Failure(error);
            }

            // 2.  Validate ticket existence
            var ticket = await _unitOfWork.Tickets.GetByIdAsync(ticketId, cancellationToken);
            if (ticket == null)
            {
                return Result<int>.NotFound($"Ticket {ticketId} not found");
            }

            // 3.  Validate max files per ticket
            if (ticket.Attachments.Count >= _settings.MaxFilesPerTicket)
            {
                return Result<int>.Failure($"Maximum number of files ({_settings.MaxFilesPerTicket}) reached for this ticket");
            }

            // 4.  Generate safe file name and path
            var storedFileName = $"{Guid.NewGuid()}_{GenerateSafeFileName(file.FileName)}";
            var filePath = await SaveFileSecurelyAsync(file, storedFileName, cancellationToken);

            // 5.  Create attachment entity (uses factory method)
            var attachmentResult = Attachment.Create(
                file.FileName,
                storedFileName,
                filePath,
                file.ContentType,
                file.Length,
                ticketId,
                userId
            );

            if (!attachmentResult.IsSuccess)
            {
                return Result<int>.Failure(attachmentResult.Error.Description ?? "Failed to create attachment entity");
            }

            var attachment = attachmentResult.Value!;

            // 6.  Add to ticket
            ticket.AddAttachment(attachment);

            // 7.  Save to database
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logFileUploadSuccess(_logger, attachment.Id, file.FileName, null);
            activity?.SetTag("attachment.id", attachment.Id);
            activity?.SetTag("success", true);

            return Result<int>.Success(attachment.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName} for user {UserId}", file.FileName, userId);
            activity?.SetTag("success", false);
            return Result<int>.Failure("An error occurred while uploading the file");
        }
    }

    public async Task<Result<AttachmentValidationResult>> ValidateFileAsync(
        FileUploadRequest file, 
        CancellationToken cancellationToken = default)
    {
        var result = new AttachmentValidationResult
        {
            FileSizeBytes = file.Length,
            DetectedMimeType = file.ContentType
        };

        // 1.  Validate file exists
        if (file.Length == 0)
        {
            return Result<AttachmentValidationResult>.Failure("File is empty");
        }

        // 2.  Validate max size
        if (file.Length > _settings.MaxFileSizeBytes)
        {
            var maxSizeMB = _settings.MaxFileSizeBytes / (1024 * 1024);
            return Result<AttachmentValidationResult>.Failure($"File size exceeds maximum allowed ({maxSizeMB} MB)");
        }

        // 3.  Validate extension
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrEmpty(extension) || !AllowedExtensions.ContainsKey(extension))
        {
            return Result<AttachmentValidationResult>.Failure($"File type '{extension}' is not allowed");
        }

        // 4.  Validate MIME type
        var allowedMimeTypes = AllowedExtensions[extension];
        if (!allowedMimeTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            result.Warnings.Add($"MIME type '{file.ContentType}' doesn't match extension '{extension}'");
        }

        // 5.  Validate magic bytes
        if (MagicBytes.ContainsKey(extension))
        {
            var isValidContent = await ValidateMagicBytesAsync(file, extension, cancellationToken);
            if (!isValidContent)
            {
                return Result<AttachmentValidationResult>.Failure("File content doesn't match its extension (possible spoofing attempt)");
            }
        }

        // 6.  Validate file name
        if (ContainsDangerousCharacters(file.FileName))
        {
            return Result<AttachmentValidationResult>.Failure("File name contains dangerous characters");
        }

        // 7.  Simulate virus scan
        result.PassedVirusScan = await SimulateVirusScanAsync(file, cancellationToken);
        if (!result.PassedVirusScan)
        {
            return Result<AttachmentValidationResult>.Failure("File failed virus scan");
        }

        result.IsValid = true;
        return Result<AttachmentValidationResult>.Success(result);
    }

    public async Task<Result<string>> GetDownloadUrlAsync(
        int attachmentId, 
        int userId, 
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return Result<string>.Success($"/api/attachments/{attachmentId}/download");
    }

    public async Task<Result> DeleteAttachmentAsync(
        int attachmentId, 
        int userId, 
        CancellationToken cancellationToken = default)
    {
        await Task.CompletedTask;
        return Result.Success();
    }

    // ==================== PRIVATE METHODS ====================

    private async Task<bool> ValidateMagicBytesAsync(FileUploadRequest file, string extension, CancellationToken cancellationToken)
    {
        if (!MagicBytes.ContainsKey(extension))
            return true;

        if (file.Content.CanSeek)
            file.Content.Position = 0;

        var buffer = new byte[8];
        var bytesRead = await file.Content.ReadAsync(buffer, 0, buffer.Length, cancellationToken);

        if (file.Content.CanSeek)
            file.Content.Position = 0;

        var expectedMagicBytes = MagicBytes[extension];
        
        foreach (var expectedBytes in expectedMagicBytes)
        {
            if (bytesRead >= expectedBytes.Length)
            {
                var matches = true;
                for (int i = 0; i < expectedBytes.Length; i++)
                {
                    if (buffer[i] != expectedBytes[i])
                    {
                        matches = false;
                        break;
                    }
                }
                if (matches) return true;
            }
        }

        return false;
    }

    private async Task<bool> SimulateVirusScanAsync(FileUploadRequest file, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
        
        var suspiciousNames = new[] { "virus", "malware", "trojan", "backdoor" };
        var fileName = file.FileName.ToLowerInvariant();
        
        return !suspiciousNames.Any(suspicious => fileName.Contains(suspicious));
    }

    private static bool ContainsDangerousCharacters(string fileName)
    {
        var dangerousChars = new[] { '<', '>', ':', '"', '|', '?', '*', '\\', '/', '\0' };
        return fileName.Any(c => dangerousChars.Contains(c));
    }

    private static string GenerateSafeFileName(string originalFileName)
    {
        var safeName = string.Join("_", originalFileName.Split(Path.GetInvalidFileNameChars()));
        
        if (safeName.Length > 100)
        {
            var extension = Path.GetExtension(safeName);
            var nameWithoutExtension = Path.GetFileNameWithoutExtension(safeName);
            safeName = nameWithoutExtension[..90] + extension;
        }

        return safeName;
    }

    private async Task<string> SaveFileSecurelyAsync(FileUploadRequest file, string fileName, CancellationToken cancellationToken)
    {
        var uploadsPath = Path.Combine(_settings.UploadPath, DateTime.UtcNow.ToString("yyyy/MM/dd"));
        Directory.CreateDirectory(uploadsPath);
        
        var filePath = Path.Combine(uploadsPath, fileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        if (file.Content.CanSeek)
            file.Content.Position = 0;
            
        await file.Content.CopyToAsync(stream, cancellationToken);
        
        return filePath;
    }
}

/// <summary>
///  Configuration for attachment service
/// </summary>
public class AttachmentSettings
{
    public const string SectionName = "AttachmentSettings";
    
    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public string UploadPath { get; set; } = "uploads";
    public bool EnableVirusScanning { get; set; } = true;
    public int MaxFilesPerTicket { get; set; } = 10;
}
