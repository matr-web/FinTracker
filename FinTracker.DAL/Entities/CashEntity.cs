namespace FinTracker.DAL.Entities;

public class CashEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserEntity? User { get; set; }

    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
}
