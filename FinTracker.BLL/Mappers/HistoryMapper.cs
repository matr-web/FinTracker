using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HistoryDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Mappers;

public class HistoryMapper
{
    /// <summary>
    /// Gets an expression that projects a HistoryEntity to a HistoryDTO for use in LINQ queries.
    /// </summary>
    /// <remarks>This projection can be used with Entity Framework or other LINQ providers to efficiently
    /// select only the required fields from the data source. The resulting HistoryDTO includes related 
    /// User information if available.</remarks>
    public static Expression<Func<HistoryEntity, HistoryDTO>> Projection =>
    h => new HistoryDTO
    {
        Id = h.Id,
        AssetName = h.AssetName,
        Ticker = h.Ticker,
        AssetType = h.AssetType,
        Operation = h.Operation,
        Quantity = h.Quantity,
        PricePerUnit = h.PricePerUnit,
        CurrencyCode = h.CurrencyCode,
        CurrencyPrice = h.CurrencyPrice,
        Description = h.Description,
        Date = h.Date,
        Profit = h.Profit,

        User = h.User != null ? new UserDTO()
        {
            Id = h.User.Id,
            Username = h.User.Username,
            Email = h.User.Email
        } : null,
    };
}
