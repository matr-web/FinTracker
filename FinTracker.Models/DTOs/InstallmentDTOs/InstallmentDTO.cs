using FinTracker.Models.DTOs.DebtDTOs;

namespace FinTracker.Models.DTOs.InstallmentDTOs;

public class InstallmentDTO
{
    public int Id { get; set; }

    public int DebtId { get; set; }
    public DebtDTO? Debt { get; set; }

    public decimal AmountLeft { get; set; }
    public int? NumberOfInstallment { get; set; }
    public DateOnly RepaymentDate { get; set; }
}
