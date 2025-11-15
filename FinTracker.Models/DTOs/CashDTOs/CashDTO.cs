using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.Models.DTOs.CashDTOs;

public class CashDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserDTO? User { get; set; }

    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }

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
