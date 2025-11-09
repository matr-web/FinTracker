namespace FinTracker.Models.DTOs.DebtDTOs;

public class BasicDebtData
{
    public int Id { get; set; }
    public decimal AmountLeft { get; set; }
    public DateOnly Date { get; set; }
}
