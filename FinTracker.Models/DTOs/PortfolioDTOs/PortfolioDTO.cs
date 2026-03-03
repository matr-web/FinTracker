using FinTracker.Models.DTOs.UserDTOs;

namespace FinTracker.Models.DTOs.PortfolioDTOs;

public class PortfolioDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public UserDTO? User { get; set; }
    public DateOnly Date { get; set; }
    public decimal ValueInvested { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalPercentageChange { get; set; }
}
