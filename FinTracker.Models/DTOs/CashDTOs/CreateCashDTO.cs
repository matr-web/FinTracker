using FinTracker.Models.Enums;

namespace FinTracker.Models.DTOs.CashDTOs;

public class CreateCashDTO
{
    public CashType CashType { get; set; }
    public decimal Amount { get; set; }

    // If the date is not provided, it will be set to today's date.
    private DateOnly? _date;
    public DateOnly? Date
    {
        get => _date ?? DateOnly.FromDateTime(DateTime.Today);
        set => _date = value;
    }
}
