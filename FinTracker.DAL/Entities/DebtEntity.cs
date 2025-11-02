namespace FinTracker.DAL.Entities;

public class DebtEntity
{
    public int Id { get; set; }

    public string? LoanPurpose { get; set; }

    public decimal Amount { get; set; }

    /// <summary>
    /// Decimal fraction for the interest rate.
    /// e.g. 0.10 is 10%.
    /// </summary>
    public decimal? InterestRateProcentage { get; set; }

    public int NumberOfInstallments { get; set; }

    public int? NumberOfInstallmentsLeft { get; set; }

    public decimal InstallmentAmount { get; set; }

    public int UserId { get; set; }

    public UserEntity? User { get; set; }
}
