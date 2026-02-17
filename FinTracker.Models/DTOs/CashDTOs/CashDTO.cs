using FinTracker.Models.DTOs.UserDTOs;

namespace FinTracker.Models.DTOs.CashDTOs;

public class CashDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserDTO? User { get; set; }

    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
}
