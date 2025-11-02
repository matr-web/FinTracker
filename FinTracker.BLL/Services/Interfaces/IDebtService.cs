using FinTracker.Models.DTOs.DebtDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface IDebtService
{
    /// <summary>
    /// Get the entire(summed up) debt for a given user.
    /// </summary>
    Task<SummedDebtDTO?> GetSummedDebt(int userId);

    /// <summary>
    /// Get all debts for a given user.
    /// </summary>
    IEnumerable<DebtDTO?> GetAllDebtsAsync(int userId);

    /// <summary>
    /// Get a single debt element with given Id.
    /// </summary>
    Task<DebtDTO?> GetSingleDebtAsync(int debtId);

    /// <summary>
    /// Add new debt for given user.
    /// </summary>
    Task<int> InsertDebtAsync(CreateDebtDTO createDebtDTO, int userId);

    /// <summary>
    /// Delete single debt.
    /// </summary>
    Task DeleteSingleDebtAsync(int debtId);

    /// <summary>
    /// Delete all debts for a given user.
    /// </summary>
    Task DeleteWholeDebtAsync(int userId);
}
