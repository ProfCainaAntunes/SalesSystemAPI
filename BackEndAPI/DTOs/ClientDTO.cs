namespace BackEndAPI.DTOs;
using System.ComponentModel.DataAnnotations;

public class ClientDTO
{
    [Required(ErrorMessage = "Name of client is required.")]
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
}