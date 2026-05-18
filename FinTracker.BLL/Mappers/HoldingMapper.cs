using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HoldingDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Mappers;

public class HoldingMapper
{
    /// <summary>
    /// Gets an expression that projects a HoldingEntity to a HoldingDTO for use in LINQ queries.
    /// </summary>
    /// <remarks>This projection can be used with Entity Framework or other LINQ providers to efficiently
    /// select only the required fields from the data source. The resulting HoldingDTO includes related User information if
    /// available.</remarks>
    public static Expression<Func<HoldingEntity, HoldingDTO>> Projection =>
    h => new HoldingDTO
    {
        Id = h.Id,
        StockName = h.StockName,
        TickerSymbol = h.TickerSymbol,
        Quantity = h.Quantity,
        BuyPrice = h.BuyPrice,
        CurrencyCode = h.CurrencyCode,
        CurrencyPrice = h.CurrencyPrice,
        UserId = h.UserId
    };
}
