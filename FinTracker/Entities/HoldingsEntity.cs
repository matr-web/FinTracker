namespace FinTracker.WebAPI.Entities;

public class HoldingsEntity
{
    public int Id { get; set; }

    public required string StockName { get; set; }

    public decimal Value { get; set; }

    public int UserId { get; set; }

    public required UserEntity User { get; set; }
}
