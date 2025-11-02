using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.DebtDTOs;

public class CreateDebtDTO
{
    public string? LoanPurpose { get; set; }
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }
    /// <summary>
    /// Decimal fraction for the interest rate.
    /// e.g. 0.10 is 10%.
    /// </summary>
    public decimal? InterestRateProcentage { get; set; }
    public int NumberOfInstallments { get; set; }

    public int? NumberOfInstallmentsLeft { get; set; }

    public decimal InstallmentAmount { get; set; }
}
