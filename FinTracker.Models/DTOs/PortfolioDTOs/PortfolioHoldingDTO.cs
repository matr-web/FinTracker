using FinTracker.Models.DTOs.UserDTOs;
using FinTracker.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.PortfolioDTOs;

/// <summary>
/// Represents a holding within a user's portfolio, containing details about the stock, its quantity, purchase price, 
/// current value, and associated currency information. This class is used to transfer data related to individual holdings in a user's portfolio.
/// </summary>
public class PortfolioHoldingDTO
{
    public int Id { get; set; }
    public required string TickerSymbol { get; set; }
    public required string StockName { get; set; }
    public decimal Quantity { get; set; }

    [DataType(DataType.Currency)]
    public decimal BuyPrice { get; set; }
    public decimal PercentageChange { get; set; }
    public decimal PercentageChangeWithCurrencyChangesCalculated { get; set; }
    [DataType(DataType.Currency)]
    public decimal CurrentPrice { get; set; }
    [DataType(DataType.Currency)]
    public decimal CurrentHoldingValue { get; set; }
    public decimal PortfolioPercentage { get; set; }

    public CurrencyCode CurrencyCode { get; set; }
    [DataType(DataType.Currency)]
    public decimal CurrencyPriceWhenBought { get; set; } = 1m;
    [DataType(DataType.Currency)]
    public decimal CurrentCurrencyPrice { get; set; } = 1m;

    public int UserId { get; set; }
    public UserDTO? User { get; set; }
}
