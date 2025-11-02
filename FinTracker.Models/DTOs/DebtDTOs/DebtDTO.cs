using FinTracker.Models.DTOs.InstallmentDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.DebtDTOs;

public class DebtDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserDTO? User { get; set; }

    public string? LoanPurpose { get; set; }
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }
    /// <summary>
    /// Decimal fraction for the interest rate.
    /// e.g. 0.10 is 10%.
    /// </summary>
    public decimal? InterestRateProcentage { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal InstallmentAmount { get; set; }

    public ICollection<InstallmentDTO>? Installments { get; set; }
}
