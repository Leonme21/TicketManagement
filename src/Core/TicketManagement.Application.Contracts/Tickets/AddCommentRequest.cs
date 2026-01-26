using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Application.Contracts.Tickets;

public class AddCommentRequest
{
    public int TicketId { get; set; }

    [Required(ErrorMessage = "El comentario no puede estar vacío")]
    [StringLength(1000, ErrorMessage = "El comentario no puede exceder 1000 caracteres")]
    public string Content { get; set; } = string.Empty;
}