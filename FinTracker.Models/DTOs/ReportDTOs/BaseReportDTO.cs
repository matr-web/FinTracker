using FinTracker.Models.Enums;

namespace FinTracker.Models.DTOs.ReportDTOs;

public class BaseReportDTO
{
    /// <summary>
    /// Gets or sets the date for the report. This property represents the specific date for which the report data is relevant.
    /// </summary>
    public DateOnly Date { get; set; }

    /// <summary>
    /// Gets or sets the type of finances being reported. This property indicates the category of finances, such as cash, PPK, debt, or investment.
    /// </summary>
    public FinancesType FinancesType { get; set; }

    /// <summary>
    /// Gets or sets the current value of the asset for the report. This property represents the monetary value of the asset at the time of the report.
    /// </summary>
    public decimal? CurrentValue { get; set; }

    /// <summary>
    /// Gets or sets the change in value of the asset for the current month.
    /// This property indicates how much the value of the asset has increased or decreased during the current month.
    /// </summary>
    public decimal? ValueChangeOneMonth { get; set; }

    /// <summary>
    /// Gets or sets the change in value of the asset over the last three months.
    /// </summary>
    public decimal? ValueChangeThreeMonths { get; set; }

    /// <summary>
    /// Gets or sets the change in value of the asset over the last six months.
    /// </summary>
    public decimal? ValueChangeSixMonths { get; set; }

    /// <summary>
    /// Gets or sets the change in value of the asset year-to-date.
    /// </summary>
    public decimal? ValueChangeYTD { get; set; }
}
