using FinTracker.Models.DTOs.UserDTOs;
using FinTracker.Models.Enums;

namespace FinTracker.Models.DTOs.CashDTOs;

public class CashDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserDTO? User { get; set; }

    public CashType CashType { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
}
