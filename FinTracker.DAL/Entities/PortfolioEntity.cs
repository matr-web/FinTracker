namespace FinTracker.DAL.Entities;

public class PortfolioEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserEntity? User { get; set; }
    public DateOnly Date { get; set; }
    public decimal ValueInvested { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalPercentageChange { get; set; }
}
