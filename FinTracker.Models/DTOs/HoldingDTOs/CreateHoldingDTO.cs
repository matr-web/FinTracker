using FinTracker.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.HoldingDTOs;

public class CreateHoldingDTO
{
    public required string StockName { get; set; }
    public required string TickerSymbol { get; set; }
    public decimal Quantity { get; set; }

    [DataType(DataType.Currency)]
    public decimal BuyPrice { get; set; }
    public CurrencyCode CurrencyCode { get; set; }

    [DataType(DataType.Currency)]
    public decimal? CurrencyPrice { get; set; }
}
