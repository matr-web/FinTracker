using FinTracker.Models.DTOs.UserDTOs;
using FinTracker.Models.Enums;

namespace FinTracker.Models.DTOs.HoldingDTOs;

/// <summary>
/// Data Transfer Object (DTO) representing a holding in a user's portfolio, 
/// containing details about the stock, its quantity, purchase price,
/// </summary>
public class HoldingDTO
{
    public int Id { get; set; }

    public required string TickerSymbol { get; set; }
    public required string StockName { get; set; }
    public decimal Quantity { get; set; }
    public decimal BuyPrice { get; set; }
    public CurrencyCode CurrencyCode { get; set; }
    public decimal CurrencyPrice { get; set; }

    public int UserId { get; set; }
    public UserDTO? User { get; set; }
}
