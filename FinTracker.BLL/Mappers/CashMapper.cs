using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.CashDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Mappers;

public class CashMapper
{
    /// <summary>
    /// Static mapping expression from DebtEntity to DebtDTO.
    /// </summary>
    public static Expression<Func<CashEntity, CashDTO>> Projection =>
    c => new CashDTO
    {
        Id = c.Id,
        UserId = c.UserId,
        Amount = c.Amount,
        Date = c.Date,

        User = c.User != null ? new UserDTO()
        {
            Id = c.User.Id,
            Username = c.User.Username,
            Email = c.User.Email
        } : null
    };
}
