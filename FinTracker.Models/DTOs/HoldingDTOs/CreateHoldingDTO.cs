namespace FinTracker.Models.DTOs.HoldingDTOs;

public class CreateHoldingDTO
{
    public required string StockName { get; set; }

    public decimal Value { get; set; }
}
