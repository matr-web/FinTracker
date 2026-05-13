using FinTracker.Models.DTOs.DebtDTOs;
using FinTracker.Models.DTOs.InstallmentDTOs;

namespace FinTracker.BLL.Services.Interfaces;

public interface IDebtService
{
    /// <summary>
    /// Retrieves a collection of summed debt information for the specified user for each month.
    /// </summary>
    /// <remarks>This method may return an empty collection if the user has no associated debt records. Ensure
    /// that the userId is valid to avoid unexpected results.</remarks>
    /// <param name="userId">The unique identifier of the user for whom the summed debt is being retrieved. Must be a positive integer.</param>
    /// <returns>An enumerable collection of SummedDebtDTO objects representing the summed debt for the user. The collection may
    /// contain null values if no debt records are found.</returns>
    Task<IEnumerable<SummedDebtDTO>?> GetSummedDebtAsync(int userId, int? periodOfTime);

    /// <summary>
    /// Retrieves all debts associated with the specified user.
    /// </summary>
    /// <remarks>This method allows for efficient querying of debts and supports further filtering and sorting
    /// operations on the returned IQueryable.</remarks>
    /// <param name="userId">The unique identifier of the user whose debts are to be retrieved. Must be a positive integer.</param>
    /// <returns>An IQueryable collection of DebtDTO objects representing the debts of the specified user. The collection may be
    /// empty if the user has no debts.</returns>
    Task<IEnumerable<DebtDTO>> GetAllDebtsAsync(int userId);

    /// <summary>
    /// Retrieves a single debt record identified by the specified debt ID.
    /// </summary>
    /// <remarks>This method may return null if no debt record matches the provided debt ID. Ensure that the
    /// debt ID is valid before calling this method.</remarks>
    /// <param name="debtId">The unique identifier of the debt to retrieve. Must be a positive integer.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a DebtDTO object if the debt is
    /// found; otherwise, null.</returns>
    Task<DebtDTO?> GetSingleDebtAsync(int userId, int debtId);

    /// <summary>
    /// Processes the repayment of a specific installment for a given debt asynchronously.
    /// </summary>
    /// <remarks>Ensure that the provided debtId refers to a valid debt and that the repayInstallmentDTO
    /// contains accurate and complete repayment details. The method does not update the debt if the repayment is
    /// invalid or fails.</remarks>
    /// <param name="debtId">The unique identifier of the debt for which the installment repayment is being made. Must correspond to an
    /// existing debt.</param>
    /// <param name="repayInstallmentDTO">An object containing the details of the repayment installment, including the amount and payment method. Must
    /// provide valid repayment information.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains a DebtDTO object with updated debt
    /// information if the repayment succeeds; otherwise, null if the repayment fails.</returns>
    Task<DebtDTO?> PayOffInstallmentAsync(int userId, int debtId, RepayInstallmentDTO repayInstallmentDTO);

    /// <summary>
    /// Inserts a new debt record asynchronously and returns the unique identifier of the created debt.
    /// </summary>
    /// <remarks>Validation is performed on the provided debt details. The method may throw exceptions if the
    /// input data is invalid.</remarks>
    /// <param name="createDebtDTO">The data transfer object containing the details of the debt to be created. Must include all required fields such
    /// as amount and description.</param>
    /// <param name="userId">The identifier of the user associated with the debt. Must be a valid user ID.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the unique identifier of the newly
    /// created debt record.</returns>
    Task<int> InsertDebtAsync(CreateDebtDTO createDebtDTO, int userId);

    /// <summary>
    /// Deletes a single debt identified by the specified debt ID asynchronously.    
    /// If no record with the specified identifier exists 
    /// or if the record does not belong to the specified user, an ArgumentOutOfRangeException is thrown.
    /// </summary>
    /// <remarks>Ensure that the specified debt ID exists before calling this method to avoid exceptions. The
    /// operation is performed asynchronously and does not return a result upon completion.</remarks>
    /// <param name="debtId">The unique identifier of the debt to be deleted. Must be a positive integer.</param>
    /// <param name="userId">The unique identifier of the user associated with the debt. Must be a positive integer.</param>
    /// <returns>A task that represents the asynchronous operation of deleting the debt.</returns>
    Task<bool> DeleteSingleDebtAsync(int userId,int debtId);

    /// <summary> 
    /// Deletes all debts associated with the specified user asynchronously.
    /// </summary>
    /// <remarks>Ensure that the specified user exists before calling this method to avoid exceptions. The
    /// operation is performed asynchronously and may throw exceptions if the userId is invalid or does not correspond
    /// to an existing user.</remarks>
    /// <param name="userId">The unique identifier of the user whose debts are to be deleted. Must be a positive integer.</param>
    /// <returns>The number of deleted debt records.</returns>
    Task<int> DeleteWholeDebtAsync(int userId);
}
