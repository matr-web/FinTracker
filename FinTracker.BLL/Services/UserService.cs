using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Http;
using FinTracker.BLL.Mappers;
using Microsoft.Extensions.Configuration;

namespace FinTracker.BLL.Services;

public class UserService : IUserService
{
    private readonly FinTrackerDbContext _context;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _config;

    public UserService(FinTrackerDbContext context, IHttpContextAccessor httpContextAccessor, IConfiguration config)
    {
        _context = context;
        _httpContextAccessor = httpContextAccessor;
        _config = config;
    }

    ///<inheritdoc cref="IUserService.UserId"/>
    public int? UserId
    {
        get 
        {
            var user = _httpContextAccessor.HttpContext?.User;

            var idString = user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(idString, out var id) ? id : null;
        }
    }

    ///<inheritdoc cref="IUserService.GenerateToken"/>
    public string GenerateToken(UserDTO userDTO)
    {
        var secretKey = _config["JwtSettings:Key"];
        var issuer = _config["JwtSettings:Issuer"];
        var audience = _config["JwtSettings:Audience"];
        var expirationDays = _config.GetValue<int>("JwtSettings:ExpirationDays", 30);

        if (string.IsNullOrEmpty(secretKey))
            throw new Exception("JWT Key is missing in configuration.");

        List<Claim> claims = new()
        {
                new Claim(ClaimTypes.NameIdentifier, userDTO.Id.ToString()),
                new Claim(ClaimTypes.Name, userDTO.Username!),
                new Claim(ClaimTypes.Email, userDTO.Email!),
            };

   
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsVeryImportant_ThisIsTheSecretKey_StoreThisSomewhereSecure_ThisIsVeryImportant"));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        var token = new JwtSecurityToken(
                "https://localhost:7191",
                "http://FinTracker.com",
                claims: claims,
                expires: DateTime.Now.AddDays(expirationDays),
                signingCredentials: creds);

        // Write and return the Token.
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        return jwt;
    }

    ///<inheritdoc cref="IUserService.RegisterUserAsync"/>
    public async Task<UserDTO?> RegisterUserAsync(RegisterUserDTO registerUserDTO)
    {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDTO.Password);

        var user = new UserEntity()
        {
            Username = registerUserDTO.Username,
            Email = registerUserDTO.Email,
            PasswordHash = passwordHash
        };

        await _context.Users.AddAsync(user);
        var result = await _context.SaveChangesAsync();

       return result > 0 ? new UserDTO
       {
           Id = user.Id,
           Username = user.Username,
           Email = user.Email
       } : null;
    }

    ///<inheritdoc cref="IUserService.GetUserByAsync"/>
    public async Task<UserDTO?> GetUserByAsync(Expression<Func<UserEntity, bool>> filterExpression)
    {
        // Get UserEntity with fulfill given requirements.
        return await _context.Users
            .Where(filterExpression)
            .Select(UserMapper.Projection) 
            .FirstOrDefaultAsync();
    }
}