using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.UserDTOs;

public class LoginUserDTO
{
    public required string UsernameOrEmail { get; set; }

    [DataType(DataType.Password)]
    public required string Password { get; set; }
}
