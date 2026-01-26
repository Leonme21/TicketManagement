using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TicketManagement.Domain.Common;
using TicketManagement.Domain.Exceptions;

namespace TicketManagement.Domain.Entities;

/// <summary>
/// Archivo adjunto a un ticket (imagen, PDF, logs, etc.)
/// </summary>
public class Attachment : BaseEntity
{
    private Attachment() { } // EF Core

    /// <summary>
    /// Constructor para crear nuevo attachment
    /// </summary>
    public Attachment(string fileName, string filePath, string contentType, long fileSize, int ticketId)
    {
        if (string.IsNullOrWhiteSpace(fileName))
            throw new DomainException("File name cannot be empty");

        if (string.IsNullOrWhiteSpace(filePath))
            throw new DomainException("File path cannot be empty");

        if (fileSize <= 0)
            throw new DomainException("File size must be greater than zero");

        if (fileSize > 10_485_760) // 10 MB
            throw new DomainException("File size cannot exceed 10 MB");

        FileName = fileName;
        FilePath = filePath;
        ContentType = contentType;
        FileSize = fileSize;
        TicketId = ticketId;
    }

    public string FileName { get; private set; } = string.Empty;
    public string FilePath { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long FileSize { get; private set; }

    // Foreign Key
    public int TicketId { get; private set; }

    // Navigation Property
    public Ticket Ticket { get; set; } = null!;

    /// <summary>
    /// Obtiene el tamaño del archivo en formato legible (KB, MB)
    /// </summary>
    public string GetFileSizeFormatted()
    {
        if (FileSize < 1024)
            return $"{FileSize} B";

        if (FileSize < 1_048_576)
            return $"{FileSize / 1024:F2} KB";

        return $"{FileSize / 1_048_576:F2} MB";
    }
}
