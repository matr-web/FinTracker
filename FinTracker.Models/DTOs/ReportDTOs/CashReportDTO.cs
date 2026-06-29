namespace FinTracker.Models.DTOs.ReportDTOs;

/// <summary>
/// Represents a data transfer object (DTO) for cash reports, extending the BaseReportDTO class.
/// </summary>
public class CashReportDTO : BaseReportDTO
{
    /// <summary>
    /// Gets or sets the average amount of deposits made.
    /// </summary>
    public decimal AverageDeposit { get; set; }

    /// <summary>
    /// Gets or sets the biggest amount of a single deposit made. 
    /// This property represents the maximum value of any individual deposit recorded in the cash area.
    /// </summary>
    public decimal BiggestSingleDeposit { get; set; }

    /// <summary>
    /// Gets or sets the date of the biggest single deposit made.
    /// </summary>
    public DateOnly BiggestSingleDepositDate { get; set; }

    /// <summary>
    /// Gets or sets the average amount of withdrawals made.
    /// </summary>
    public decimal AverageWithdrawal { get; set; }

    /// <summary>
    /// Gets or sets the biggest amount of a single withdrawal made.
    /// This property represents the maximum value of any individual withdrawal recorded in the cash area.
    /// </summary>
    public decimal BiggestSingleWithdrawal { get; set; }

    /// <summary>
    /// Gets or sets the date of the biggest single withdrawal made.
    /// </summary>
    public DateOnly BiggestSingleWithdrawalDate { get; set; }
}
