using FinTracker.Models.Enums;

namespace FinTracker.DAL.Entities;

public class HoldingEntity
{
    public int Id { get; set; }
    public required string TickerSymbol { get; set; }

    public required string StockName { get; set; }
    public double Quantity { get; set; }
    public decimal BuyPrice { get; set; }
    public Currency Currency { get; set; }

    public int UserId { get; set; }
    public UserEntity? User { get; set; }
}
