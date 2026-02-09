using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TicketManagement.Application.Common.Interfaces;

namespace TicketManagement.WebApi.Controllers;

[Authorize]
public class AttachmentsController : ApiControllerBase
{
    private readonly IAttachmentService _attachmentService;

    public AttachmentsController(IAttachmentService attachmentService)
    {
        _attachmentService = attachmentService;
    }

    [HttpPost("tickets/{ticketId}/attachments")]
    [EnableRateLimiting("file-upload")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [RequestSizeLimit(52428800)] // 50 MB limit
    public async Task<IActionResult> UploadAttachment(int ticketId, IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0) return BadRequest(new { error = "No file provided" });

        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(userIdClaim, out var userId)) return Unauthorized(new { error = "User not authenticated" });

        var fileRequest = new FileUploadRequest
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            Content = file.OpenReadStream()
        };

        var result = await _attachmentService.UploadAttachmentAsync(fileRequest, ticketId, userId, cancellationToken);
        if (result.IsSuccess)
        {
            return CreatedAtAction(nameof(GetAttachment), new { attachmentId = result.Value }, new { id = result.Value, message = "File uploaded successfully" });
        }

        return result.Error switch
        {
            var error when error.Description.Contains("not found") => NotFound(new { error }),
            var error when error.Description.Contains("permission") || error.Description.Contains("forbidden") => Forbid(),
            var error when error.Description.Contains("size") || error.Description.Contains("large") => StatusCode(413, new { error }),
            var error when error.Description.Contains("type") || error.Description.Contains("format") => BadRequest(new { error }),
            _ => BadRequest(new { error = result.Error })
        };
    }

    [HttpGet("attachments/{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAttachment(int attachmentId, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);
        
        var result = await _attachmentService.GetDownloadUrlAsync(attachmentId, userId, cancellationToken);
        if (result.IsSuccess) return Ok(new { downloadUrl = result.Value });

        return result.Error switch
        {
            var error when error.Description.Contains("not found") => NotFound(new { error }),
            var error when error.Description.Contains("permission") => Forbid(),
            _ => BadRequest(new { error = result.Error })
        };
    }

    [HttpDelete("attachments/{attachmentId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteAttachment(int attachmentId, CancellationToken cancellationToken = default)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int.TryParse(userIdClaim, out var userId);

        var result = await _attachmentService.DeleteAttachmentAsync(attachmentId, userId, cancellationToken);
        if (result.IsSuccess) return NoContent();

        return result.Error switch
        {
            var error when error.Description.Contains("not found") => NotFound(new { error }),
            var error when error.Description.Contains("permission") => Forbid(),
            _ => BadRequest(new { error = result.Error })
        };
    }

    [HttpPost("attachments/validate")]
    public async Task<IActionResult> ValidateFile(IFormFile file, CancellationToken cancellationToken = default)
    {
        if (file == null || file.Length == 0) return BadRequest(new { error = "No file provided" });

        var fileRequest = new FileUploadRequest
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Length = file.Length,
            Content = file.OpenReadStream()
        };

        var result = await _attachmentService.ValidateFileAsync(fileRequest, cancellationToken);
        if (result.IsSuccess) return Ok(result.Value);

        return BadRequest(new { error = result.Error });
    }
}
