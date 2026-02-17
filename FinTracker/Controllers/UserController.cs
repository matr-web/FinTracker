using FinTracker.BLL.Services.Interfaces;
using FinTracker.Models.DTOs.UserDTOs;
using Microsoft.AspNetCore.Cors;
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

    [HttpPost("Register")]
    public async Task<ActionResult> RegisterAsync([FromBody] RegisterUserDTO registerUserDTO)
    {
        var userDTO = await _userService.GetUserByAsync(u => u.Username == registerUserDTO.Username || u.Email == registerUserDTO.Email);

        if (userDTO != null)
        {
            return BadRequest("User with given User Name or Email already exists.");
        }

        var registeredUserDTO = await _userService.RegisterUserAsync(registerUserDTO);

        if (registeredUserDTO == null)
        {
            return NotFound();
        }

        return Created($"User/{registeredUserDTO.Id}", registeredUserDTO);
    }

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
