using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Services.Interfaces;

public interface IUserService
{

    int? UserId { get; }

    /// <summary>
    /// Get a specific User that fulfill given filterExpression.
    /// </summary>
    Task<UserDTO?> GetUserByAsync(Expression<Func<UserEntity, bool>> filterExpression);

    /// <summary>
    /// Register User and save him in the DB.
    /// </summary>
    Task<UserDTO?> RegisterUserAsync(RegisterUserDTO registerUserDTO);

    /// <summary>
    /// Generate a new Token for a User that wants to loggin.
    /// </summary>
    string GenerateToken(UserDTO userDTO);
}
