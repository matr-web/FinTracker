using FinTracker.Models.DTOs.UserDTOs;
using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.HoldingDTOs;

public class HoldingDTO
{
    public int Id { get; set; }

    public required string StockName { get; set; }

    [DataType(DataType.Currency)]
    public decimal Value { get; set; }

    public int UserId { get; set; }
    public UserDTO? User { get; set; }
}
