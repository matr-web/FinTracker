namespace FinTracker.DAL.Entities;

public class DebtEntity
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserEntity? User { get; set; }

    public string? LoanPurpose { get; set; }
    public decimal Amount { get; set; }
    public decimal? InterestRateProcentage { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal InstallmentAmount { get; set; }
    public DateOnly Date { get; set; }
    public bool IsPaidOff { get; set; } = false;

    public ICollection<InstallmentEntity>? Installments { get; set; }
}
