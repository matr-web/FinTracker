using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.CashDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Mappers;

public class CashMapper
{
    /// <summary>
    /// Gets an expression that projects a CashEntity to a CashDTO for use in LINQ queries.
    /// </summary>
    /// <remarks>This projection can be used with Entity Framework or other LINQ providers to efficiently
    /// select only the required fields from the data source. The resulting CashDTO includes related User information if
    /// available.</remarks>
    public static Expression<Func<CashEntity, CashDTO>> Projection =>
    c => new CashDTO
    {
        Id = c.Id,
        UserId = c.UserId,
        Amount = c.Amount,
        CashType = c.CashType,
        Date = c.Date,

        User = c.User != null ? new UserDTO()
        {
            Id = c.User.Id,
            Username = c.User.Username,
            Email = c.User.Email
        } : null
    };
}


