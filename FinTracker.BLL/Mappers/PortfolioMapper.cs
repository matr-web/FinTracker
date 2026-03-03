using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.PortfolioDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Mappers;

public class PortfolioMapper
{
    /// <summary>
    /// Static mapping expression from DebtEntity to DebtDTO.
    /// </summary>
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