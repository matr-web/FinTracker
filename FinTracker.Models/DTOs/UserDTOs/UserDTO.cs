using FinTracker.Models.DTOs.DebtDTOs;
using FinTracker.Models.DTOs.HistoryDTOs;
using FinTracker.Models.DTOs.HoldingDTOs;
using System.ComponentModel.DataAnnotations;

namespace FinTracker.Models.DTOs.UserDTOs;

public class UserDTO
{
    public int Id { get; set; }

    public string? Username { get; set; }
    [DataType(DataType.EmailAddress)]
    public string? Email { get; set; }
    public string? PasswordHash { get; set; }

    public ICollection<HistoryDTO>? History { get; set; }
    public ICollection<HoldingDTO>? Holdings { get; set; }
    public ICollection<DebtDTO>? Debts { get; set; }
}
