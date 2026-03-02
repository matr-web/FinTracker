using FinTracker.Models.DTOs.HoldingDTOs;

namespace FinTracker.Models.ViewModels;

public class PortfolioViewModel
{
    /// <summary>
    /// Gets or sets the total amount of money invested in the Portfolio.
    /// </summary>
    public decimal ValueInvested { get; set; }
    /// <summary>
    /// Gets or sets the total value of the portfolio with calculated changes which occurred in each holding.
    /// </summary>
    public decimal TotalValue { get; set; }
    /// <summary>
    /// Gets or sets the total percentage change of whole portfolio.
    /// </summary>
    /// <remarks>This property is useful for analyzing the overall performance trend of the portfolio, as it
    /// reflects the cumulative percentage change in value. A positive value indicates growth, while a negative value
    /// indicates a decline.</remarks>
    public decimal TotalPercentageChange { get; set; }
    /// <summary>
    /// Gets or sets the collection of holdings represented by the HoldingDTO objects that are right now in the portfolio.
    /// </summary>
    /// <remarks>This property may return null if no holdings are available. Each HoldingDTO contains detailed
    /// information about an individual holding.</remarks>
    public IEnumerable<HoldingDTO>? Holdings { get; set; }
}
