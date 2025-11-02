namespace FinTracker.DAL.Entities;

public class HoldingEntity
{
    public int Id { get; set; }

    public required string StockName { get; set; }
    public decimal Value { get; set; }

    public int UserId { get; set; }
    public UserEntity? User { get; set; }
}
