using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.UserDTOs;

public class RegisterUserDTO
{
    public required string Username { get; set; }

    [DataType(DataType.EmailAddress)]
    public required string Email { get; set; }

    [MinLength(6)]
    public required string Password { get; set; }
}
