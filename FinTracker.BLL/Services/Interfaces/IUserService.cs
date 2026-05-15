using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Services.Interfaces;

public interface IUserService
{

    /// <summary>
    /// Unique identifier for the authenticated user. 
    /// </summary>
    /// <remarks>Can be null if the user is not authenticated.</remarks>
    int? UserId { get; }

    /// <summary>
    /// Get User by a filter expression. This is used for login and other operations where we need to find a user by a specific condition.
    /// </summary>
    /// <param name="filterExpression">Filter expression</param>
    /// <returns>User DTO or null if not found</returns>
    Task<UserDTO?> GetUserByAsync(Expression<Func<UserEntity, bool>> filterExpression);

    /// <summary>
    /// Add a new User to the database. This is used for registration. 
    /// </summary>
    /// <param name="registerUserDTO">The DTO containing the user registration information</param>
    /// <returns>A UserDTO if the registration is successful, or null if it fails (e.g., if the username or email is already taken).</returns>
    Task<UserDTO?> RegisterUserAsync(RegisterUserDTO registerUserDTO);

    /// <summary>
    /// Generate a JWT token for the authenticated user. This is used for login and authentication purposes. 
    /// The token will contain the user's ID and other relevant information, and it will be signed to ensure its integrity and authenticity.
    /// </summary>
    /// <param name="userDTO">The DTO containing the user information</param>
    /// <returns></returns>
    string GenerateToken(UserDTO userDTO);
}
