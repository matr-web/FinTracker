using FinTracker.Models.DTOs.DebtDTOs;
using FinTracker.Models.DTOs.InstallmentDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface IDebtService
{
    /// <summary>
    /// Get data on a user's total (summed) debt for each month in which they were in debt.
    /// </summary>
    IEnumerable<SummedDebtDTO?> GetSummedDebt(int userId);

    /// <summary>
    /// Get all debts for a given user.
    /// </summary>
    IQueryable<DebtDTO?> GetAllDebts(int userId);

    /// <summary>
    /// Get a single debt element with given Id.
    /// </summary>
    Task<DebtDTO?> GetSingleDebtAsync(int debtId);

    /// <summary>
    /// Repay a installment of given debt.
    /// </summary>
    Task<DebtDTO?> PayOffInstallment(RepayInstallmentDTO repayInstallmentDTO);

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
