namespace FinTracker.Models.DTOs.DebtDTOs;

public class SummedDebtDTO
{
    public decimal AmountLeft { get; set; }
    public DateOnly RepaymentDate { get; set; }
}
