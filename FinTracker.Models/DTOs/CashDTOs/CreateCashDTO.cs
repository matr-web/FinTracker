namespace FinTracker.Models.DTOs.CashDTOs;

public class CreateCashDTO
{
    public decimal Amount { get; set; }
    public DateOnly? Date { get; set; }
}
