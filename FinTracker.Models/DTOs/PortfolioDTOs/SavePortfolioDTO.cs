namespace FinTracker.Models.DTOs.PortfolioDTOs;

public class SavePortfolioDTO
{
    public DateOnly Date { get; set; }
    public decimal ValueInvested { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalPercentageChange { get; set; }
}
