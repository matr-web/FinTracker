namespace FinTracker.Models.DTOs.UserDTOs;

public class LoginUserDTO
{
    public required string UsernameOrEmail { get; set; }

    public required string Password { get; set; }
}
