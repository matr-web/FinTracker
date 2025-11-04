namespace FinTracker.Models.DTOs.InstallmentDTOs;

public class RepayInstallmentDTO
{
    public int DebtId { get; set; }
    public DateOnly? RepaymentDate { get; set; }
}
