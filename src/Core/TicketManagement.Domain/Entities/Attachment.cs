using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Domain.Entities;

/// <summary>
///  Entidad Attachment refactorizada con validaciones de seguridad
/// Representa un archivo adjunto a un ticket con metadatos completos
/// </summary>
public class Attachment : BaseEntity, ISoftDeletable
{
    // ==================== CONSTANTS ====================
    public const int MaxFileNameLength = 255;
    public const int MaxContentTypeLength = 100;
    public const long MaxFileSizeBytes = 50 * 1024 * 1024; // 50 MB
    
    // ==================== CONSTRUCTORS ====================
    private Attachment() { } // EF Core

    private Attachment(
        string originalFileName,
        string storedFileName,
        string filePath,
        string contentType,
        long fileSizeBytes,
        int ticketId,
        int uploadedById)
    {
        OriginalFileName = originalFileName;
        StoredFileName = storedFileName;
        FilePath = filePath;
        ContentType = contentType;
        FileSizeBytes = fileSizeBytes;
        TicketId = ticketId;
        UploadedById = uploadedById;
        IsScanned = false;
    }

    // ==================== FACTORY METHOD ====================
    
    /// <summary>
    /// Factory Method para crear un nuevo Attachment
    /// </summary>
    public static Result<Attachment> Create(
        string originalFileName,
        string storedFileName,
        string filePath,
        string contentType,
        long fileSizeBytes,
        int ticketId,
        int uploadedById)
    {
        try
        {
            ValidateFileName(originalFileName);
            ValidateFileName(storedFileName);
            ValidateFilePath(filePath);
            ValidateContentType(contentType);
            ValidateFileSize(fileSizeBytes);
            ValidateTicketId(ticketId);
            ValidateUserId(uploadedById);

            var attachment = new Attachment(
                originalFileName,
                storedFileName,
                filePath,
                contentType,
                fileSizeBytes,
                ticketId,
                uploadedById);
            
            return Result<Attachment>.Success(attachment);
        }
        catch (DomainException ex)
        {
            return Result<Attachment>.Failure(ex.Message);
        }
    }

    // ==================== PROPERTIES ====================

    public string OriginalFileName { get; private set; } = string.Empty;
    public string StoredFileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSizeBytes { get; private set; }
    public bool IsScanned { get; private set; }
    public bool? ScanResult { get; private set; }
    public DateTime? ScannedAt { get; private set; }

    // Foreign Keys
    public int TicketId { get; private set; }
    public int UploadedById { get; private set; }

    // Navigation Properties
    public Ticket Ticket { get; private set; } = null!;
    public User UploadedBy { get; private set; } = null!;

    // Soft Delete
    public bool IsDeleted { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string? DeletedBy { get; set; }

    // Computed Properties
    public string FileSizeFormatted => FormatFileSize(FileSizeBytes);
    public string FileExtension => Path.GetExtension(OriginalFileName).ToLowerInvariant();
    public bool IsSafe => IsScanned && ScanResult == true;

    // ==================== BUSINESS LOGIC ====================
    
    public void MarkAsScanned(bool isClean, DateTime? scannedAt = null)
    {
        IsScanned = true;
        ScanResult = isClean;
        ScannedAt = scannedAt ?? DateTime.UtcNow;
    }

    public bool IsImage()
    {
        var imageTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/bmp", "image/webp" };
        return imageTypes.Contains(ContentType, StringComparer.OrdinalIgnoreCase);
    }

    public bool IsDocument()
    {
        var documentTypes = new[] 
        { 
            "application/pdf", 
            "application/msword",
            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            "application/vnd.ms-excel",
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "text/plain"
        };
        return documentTypes.Contains(ContentType, StringComparer.OrdinalIgnoreCase);
    }

    // ==================== VALIDATIONS ====================
    
    private static void ValidateFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("File name cannot be empty");
        
        if (fileName.Length > MaxFileNameLength)
            throw new DomainException($"File name cannot exceed {MaxFileNameLength} characters");
    }
    
    private static void ValidateFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new DomainException("File path cannot be empty");
    }
    
    private static void ValidateContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType))
            throw new DomainException("Content type cannot be empty");
        
        if (contentType.Length > MaxContentTypeLength)
            throw new DomainException($"Content type cannot exceed {MaxContentTypeLength} characters");
    }
    
    private static void ValidateFileSize(long fileSizeBytes)
    {
        if (fileSizeBytes <= 0)
            throw new DomainException("File size must be positive");
        
        if (fileSizeBytes > MaxFileSizeBytes)
            throw new DomainException($"File size cannot exceed {MaxFileSizeBytes / (1024 * 1024)} MB");
    }
    
    private static void ValidateTicketId(int ticketId)
    {
        if (ticketId <= 0)
            throw new DomainException("Ticket ID must be positive");
    }
    
    private static void ValidateUserId(int userId)
    {
        if (userId <= 0)
            throw new DomainException("User ID must be positive");
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        
        return $"{len:0.##} {sizes[order]}";
    }

    [Obsolete("Use OriginalFileName instead")]
    public string FileName => OriginalFileName;
    
    [Obsolete("Use FileSizeBytes instead")]
    public long FileSize => FileSizeBytes;
}
