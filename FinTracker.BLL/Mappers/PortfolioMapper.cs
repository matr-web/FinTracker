using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.PortfolioDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Mappers;

public class PortfolioMapper
{
    /// <summary>
    /// Gets an expression that projects a PortfolioEntity to a PortfolioDTO for use in LINQ queries.
    /// </summary>
    /// <remarks>This projection can be used with Entity Framework or other LINQ providers to efficiently
    /// select only the required fields from the data source. The resulting PortfolioDTO includes related User information if
    /// available.</remarks>
    public static Expression<Func<PortfolioEntity, PortfolioDTO>> Projection =>
    p => new PortfolioDTO
    {
        Id = p.Id,
        UserId = p.UserId,
        Date = p.Date,
        ValueInvested = p.ValueInvested,
        TotalValue = p.TotalValue,
        TotalPercentageChange = p.TotalPercentageChange,

        User = p.User != null ? new UserDTO()
        {
            Id = p.User.Id,
            Username = p.User.Username,
            Email = p.User.Email
        } : null
    };
}