using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HistoryDTOs;
using FinTracker.Models.DTOs.HoldingDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace FinTracker.BLL.Services;

public class UserService : IUserService
{
    private readonly FinTrackerDbContext _context;

    public UserService(FinTrackerDbContext context)
    {
        _context = context;
    }

    public string GenerateToken(UserDTO userDTO)
    {
        List<Claim> claims = new()
        {
                new Claim(ClaimTypes.NameIdentifier, userDTO.Id.ToString()),
                new Claim(ClaimTypes.Name, userDTO.Username!),
                new Claim(ClaimTypes.Email, userDTO.Email!),
            };

        // Create Key.
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("ThisIsVeryImportant_ThisIsTheSecretKey_StoreThisSomewhereSecure_ThisIsVeryImportant"));

        // Signing Credentials.
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

        // Generate Token.
        var token = new JwtSecurityToken(
                "https://localhost:7191",
                "http://FinTracker.com",
                claims: claims,
                expires: DateTime.Now.AddDays(30),
                signingCredentials: creds);

        // Write the Token.
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        // Return it.
        return jwt;
    }

    public async Task<UserDTO?> RegisterUserAsync(RegisterUserDTO registerUserDTO)
    {
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(registerUserDTO.Password);

        var user = new UserEntity()
        {
            Username = registerUserDTO.Username,
            Email = registerUserDTO.Email,
            PasswordHash = passwordHash
        };

        try
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();

            var userEntity = _context.Users.Single(u => u.Email == user.Email);

            return new UserDTO
            {
                Id = userEntity.Id,
                Username = userEntity.Username,
                Email = userEntity.Email,
                PasswordHash = userEntity.PasswordHash
            };
        }
        catch
        {
            throw;
        }
    }

    public async Task<UserDTO?> GetUserByAsync(Expression<Func<UserEntity, bool>> filterExpression)
    {
        // Get UserEntity with fulfill given requirements.
        var userEntity = await _context.Users.
            Include(u => u.History)
            .Include(u => u.Holdings)
            .FirstOrDefaultAsync(filterExpression);

        // If no such User exists, return null.
        if (userEntity == null)
        {
            return null;
        }

        // Map it to DTO and return.
        return new UserDTO
        {
            Id = userEntity.Id,
            Username = userEntity.Username,
            Email = userEntity.Email,
            PasswordHash = userEntity.PasswordHash,
            History = userEntity.History != null ?
            userEntity.History.Select(h => new HistoryDTO
            {
                AssetName = h.AssetName,
                Ticker = h.Ticker,
                AssetType = h.AssetType,
                Operation = h.Operation,
                Quantity = h.Quantity,
                PricePerUnit = h.PricePerUnit,
                Currency = h.Currency,
                CurrencyPrice = h.CurrencyPrice,
                Description = h.Description,
                Date = h.Date,
                Profit = h.Profit,
                UserId = userEntity.Id
            }).ToList() : null,
            Holdings = userEntity.Holdings != null ?
            userEntity.Holdings.Select(h => new HoldingDTO
            {
                Id = h.Id,
                StockName = h.StockName,
                Value = h.Value,
                UserId = h.UserId
            }).ToList() : null
        }; 
    }
}