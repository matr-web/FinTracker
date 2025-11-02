using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.HoldingDTOs;

public class CreateHoldingDTO
{
    public required string StockName { get; set; }

    [DataType(DataType.Currency)]
    public decimal Value { get; set; }
}
