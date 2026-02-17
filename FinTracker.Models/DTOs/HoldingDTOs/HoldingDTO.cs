using FinTracker.Models.DTOs.UserDTOs;
using FinTracker.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.HoldingDTOs;

public class HoldingDTO
{
    public int Id { get; set; }
    public required string TickerSymbol { get; set; }

    public required string StockName { get; set; }
    public double Quantity { get; set; }

    [DataType(DataType.Currency)]
    public decimal BuyPrice { get; set; }
    public Currency Currency { get; set; }

    public int UserId { get; set; }
    public UserDTO? User { get; set; }
}
