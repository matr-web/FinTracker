using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.UserDTOs;
using Microsoft.AspNetCore.Mvc;

namespace FinTracker.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    /// <summary>
    /// Retrieves a single user record by its unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user record to retrieve.</param>
    /// <returns>An user DTO record if found; otherwise, a NotFound result.</returns>
    [HttpGet("{id}")]
    public async Task<ActionResult<UserDTO>> GetSingleUserByIdAsync([FromRoute] int id)
    {
        var userDTO = await _userService.GetUserByAsync(u => u.Id == id);

        if (userDTO == null)
            return NotFound();

        return Ok(userDTO);
    }

    /// <summary>
    /// Registers a new user in the system. It checks if a user with the same username or email already exists, 
    /// and if not, it creates a new user record. If the registration is successful, it returns the created user record; 
    /// otherwise, it returns an appropriate error response.
    /// </summary>
    /// <param name="registerUserDTO">The data transfer object containing the details of the user to register.</param>
    /// <returns>A response with the created user if successful; otherwise, an error response.</returns>
    [HttpPost("Register")]
    public async Task<ActionResult> RegisterAsync([FromBody] RegisterUserDTO registerUserDTO)
    {
        var userDTO = await _userService.GetUserByAsync(u => u.Username == registerUserDTO.Username || u.Email == registerUserDTO.Email);

        if (userDTO != null)
            return BadRequest("User with given User Name or Email already exists.");

        var registeredUserDTO = await _userService.RegisterUserAsync(registerUserDTO);

        if (registeredUserDTO == null)
            return NotFound();

        return CreatedAtAction("GetSingleUserById", new { id = registeredUserDTO.Id }, registeredUserDTO);
    }

    /// <summary>
    /// Logins a user by verifying the provided credentials (username/email and password). 
    /// If the credentials are valid, it generates a JWT token for the user and returns it in the response. 
    /// If the credentials are invalid, it returns an appropriate error response.
    /// </summary>
    /// <param name="loginUserDTO">The data transfer object containing the login credentials.</param>
    /// <returns>A response with the generated token if successful; otherwise, an error response.</returns>
    [HttpPost("Login")]
    public async Task<ActionResult> LoginAsync([FromBody] LoginUserDTO loginUserDTO)
    {
        var userWithHashDTO = await _userService.GetUserByAsync(u => u.Email == loginUserDTO.UsernameOrEmail
            || u.Username == loginUserDTO.UsernameOrEmail);

        if (userWithHashDTO != null && BCrypt.Net.BCrypt.Verify(loginUserDTO.Password, userWithHashDTO.PasswordHash))
        {
            var token = _userService.GenerateToken(userWithHashDTO);

            return Ok(token);
        }

        return BadRequest("Wrong Username or Password.");
    }
}
