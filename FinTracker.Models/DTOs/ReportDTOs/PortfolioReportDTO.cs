namespace FinTracker.Models.DTOs.ReportDTOs;

/// <summary>
/// Represents a data transfer object (DTO) for portfolio reports, extending the BaseReportDTO class.
/// </summary>
public class PortfolioReportDTO : BaseReportDTO
{
    /// <summary>
    /// Gets or sets the total value invested in the portfolio. 
    /// This property represents the cumulative amount of money that has been invested across all assets in the portfolio.
    /// </summary>
    public decimal TotalValueInvested { get; set; }

    /// <summary>
    /// Gets or sets the total percentage change in the portfolio's value.
    /// </summary>
    public decimal TotalPercentageChange { get; set; }

    /// <summary>
    /// Gets or sets the value invested in the portfolio over the last month.
    /// </summary>
    public decimal ValueInvestedOneMonth { get; set; }

    /// <summary>
    /// Gets or sets the value invested in the portfolio over the last three months.
    /// </summary>
    public decimal ValueInvestedThreeMonths { get; set; }

    /// <summary>
    /// Gets or sets the value invested in the portfolio over the last six months.
    /// </summary>
    public decimal ValueInvestedSixMonths { get; set; }

    /// <summary>
    /// Gets or sets the value invested in the portfolio year-to-date.
    /// </summary>
    public decimal ValueInvestedYTD { get; set; }
}
