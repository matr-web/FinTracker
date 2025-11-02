namespace FinTracker.DAL.Entities;

/// <summary>
/// Data abouta debt, that changes with time.
/// </summary>
public class InstallmentEntity
{
    public int Id { get; set; }

    public int DebtId { get; set; }
    public DebtEntity? Debt { get; set; } 

    public decimal AmountLeft { get; set; }
    public int? NumberOfInstallment { get; set; }
    public DateOnly RepaymentDate { get; set; }
}
