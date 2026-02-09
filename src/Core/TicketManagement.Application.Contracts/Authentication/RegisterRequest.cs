using System.ComponentModel.DataAnnotations;

namespace TicketManagement.Application.Contracts.Authentication;

/// <summary>
/// Request para registro de nuevo usuario
/// </summary>
public class RegisterRequest
{
    [Required(ErrorMessage = "El nombre es requerido")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es requerido")]
    [EmailAddress(ErrorMessage = "Formato de correo inválido")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es requerida")]
    [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    public string Password { get; set; } = string.Empty;
    
    [Required(ErrorMessage = "La confirmacion de la contraseña es requerida")]
    [Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
