namespace FinTracker.Models.DTOs.ReportDTOs;

/// <summary>
/// Represents a data transfer object (DTO) for debt reports, extending the BaseReportDTO class.
/// </summary>
public class DebtReportDTO : BaseReportDTO
{
    /// <summary>
    /// Gets or sets the average amount of installments paid towards the debt.
    /// </summary>
    public decimal AverageInstallmentAmount { get; set; }
}
