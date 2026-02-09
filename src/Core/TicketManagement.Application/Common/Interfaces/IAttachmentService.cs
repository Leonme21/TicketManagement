using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using TicketManagement.Domain.Common;

namespace TicketManagement.Application.Common.Interfaces;

/// <summary>
///  Service interface para manejo seguro de archivos adjuntos
/// Incluye validaciones de seguridad, antivirus y almacenamiento
/// </summary>
public interface IAttachmentService
{
    /// <summary>
    /// Valida y sube un archivo adjunto de forma segura
    /// </summary>
    Task<Result<int>> UploadAttachmentAsync(
        FileUploadRequest file, 
        int ticketId, 
        int userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Obtiene la URL segura para descargar un archivo
    /// </summary>
    Task<Result<string>> GetDownloadUrlAsync(
        int attachmentId, 
        int userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Elimina un archivo adjunto (soft delete)
    /// </summary>
    Task<Result> DeleteAttachmentAsync(
        int attachmentId, 
        int userId, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Valida un archivo sin subirlo (para validacin previa)
    /// </summary>
    Task<Result<AttachmentValidationResult>> ValidateFileAsync(
        FileUploadRequest file, 
        CancellationToken cancellationToken = default);
}

/// <summary>
///  Request para subida de archivos (abstrae IFormFile)
/// </summary>
public class FileUploadRequest
{
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required long Length { get; init; }
    public required Stream Content { get; init; }
}

/// <summary>
/// Resultado de validacin de archivo
/// </summary>
public class AttachmentValidationResult
{
    public bool IsValid { get; set; }
    public string? ErrorMessage { get; set; }
    public string DetectedMimeType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public bool PassedVirusScan { get; set; }
    public List<string> Warnings { get; set; } = new();
}
