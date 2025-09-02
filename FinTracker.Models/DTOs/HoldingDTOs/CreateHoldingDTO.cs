using FinTracker.Models.DTOs.UserDTOs;

namespace FinTracker.Models.DTOs.HoldingDTOs;

public class CreateHoldingDTO
{
    public required string StockName { get; set; }

    public decimal Value { get; set; }

    public int UserId { get; set; }

    public UserDTO? User { get; set; }
}
