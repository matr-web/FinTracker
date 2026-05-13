using FinTracker.Models.DTOs.CashDTOs;
using FinTracker.Models.Enums;

namespace FinTracker.BLL.Services.Interfaces;

public interface ICashService
{
    /// <summary>
    /// Retrieves a queryable collection of cash amount records for a specified user and cash type(SavingAccount or PPK),
    /// optionally limited to a given number of most recent records.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cash history is to be retrieved.</param>
    /// <param name="cashType">The type of cash transactions to include in the history.</param>
    /// <param name="monthsCount">The maximum number of months to return. If null, all matching records are returned.</param>
    /// <returns>An IQueryable of CashDTO objects representing the user's saved cash amount history, filtered by the specified
    /// cash type(SavingAccount or PPK) and limited by the period of time if provided.</returns>
    Task<IEnumerable<CashDTO>> GetCashHistoryAsync(int userId, CashType cashType, int? monthsCount);

    /// <summary>
    /// Gets the most recent cash amount record for a specified user and cash type(SavingAccount or PPK). 
    /// If there are multiple records with the same date, the one with the highest amount will be returned.
    /// If there are no records for the specified user and cash type, null will be returned.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cash record is to be retrieved.</param>
    /// <param name="cashType">The type of cash record to retrieve (SavingAccount or PPK).</param>
    /// <returns>A CashDTO representing the most recent cash record for the specified user and cash type, or null if no records are found.</returns>
    Task<CashDTO?> GetCurrentCashAsync(int userId, CashType cashType);

    /// <summary>
    /// Gets a single cash amount record by its unique identifier. If there is no record with the specified identifier, null will be returned.
    /// </summary>
    /// <param name="cashId">The unique identifier of the cash record to retrieve.</param>
    /// <returns>A CashDTO representing the cash record with the specified identifier, or null if no record is found.</returns>
    Task<CashDTO?> GetSingleCashValueAsync(int userId, int cashId);

    /// <summary>
    /// Asynchronously inserts a new cash record for the specified user, replacing any existing record for the same
    /// user, cash type, and month.
    /// </summary>
    /// <remarks>If a cash record already exists for the specified user, cash type, and month, it will be
    /// removed before inserting the new record. Only one cash record per user, cash type, and month is
    /// maintained.</remarks>
    /// <param name="userId">The unique identifier of the user for whom the cash record is to be inserted.</param>
    /// <param name="createCashDTO">An object containing the details of the cash record to insert, including amount, cash type, and optional date.
    /// The amount must be non-negative, and the date, if provided, cannot be in the future.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the identifier of the newly inserted
    /// cash record.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the amount is negative or if the provided date is in the future.</exception>
    Task<int> InsertCashAsync(int userId, CreateCashDTO createCashDTO);

    /// <summary>
    /// Asynchronously deletes a cash record identified by its unique identifier. 
    /// If no record with the specified identifier exists 
    /// or if the record does not belong to the specified user, an ArgumentOutOfRangeException is thrown.
    /// </summary>
    /// <param name="cashId">The unique identifier of the cash record to delete.</param>
    /// <param name="userId">The unique identifier of the user who owns the cash record.</param>    
    /// <returns>A task that represents the asynchronous operation. The task result contains a boolean indicating whether the deletion was successful.</returns>
    /// <exception cref="ArgumentException">Thrown if no cash record with the specified identifier exists
    /// or if the record does not belong to the specified user.</exception>
    Task<bool> DeleteCashAsync(int userId, int cashId);
}
