using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.PortfolioDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Mappers;

public class PortfolioHoldingMapper
{
    /// <summary>
    /// Gets an expression that projects a HoldingEntity to a PortfolioHoldingDTO for use in LINQ queries.
    /// </summary>
    /// <remarks>This projection can be used with Entity Framework or other LINQ providers to efficiently
    /// select only the required fields from the data source. The resulting HoldingDTO includes related User information if
    /// available.</remarks>
    public static Expression<Func<HoldingEntity, PortfolioHoldingDTO>> Projection =>
    h => new PortfolioHoldingDTO
    {
        Id = h.Id,
        StockName = h.StockName,
        TickerSymbol = h.TickerSymbol,
        Quantity = h.Quantity,
        BuyPrice = h.BuyPrice,
        CurrencyCode = h.CurrencyCode,
        CurrencyPriceWhenBought = h.CurrencyPrice,
        UserId = h.UserId
    };
}
