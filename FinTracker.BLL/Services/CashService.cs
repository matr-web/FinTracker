using FinTracker.BLL.Mappers;
using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.CashDTOs;
using FinTracker.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.BLL.Services;

public class CashService : ICashService
{
    private readonly FinTrackerDbContext _dbContext;
    public CashService(FinTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Retrieves a queryable collection of cash amount records for a specified user and cash type(SavingAccount or PPK),
    /// optionally limited to a given number of most recent records.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cash history is to be retrieved.</param>
    /// <param name="cashType">The type of cash transactions to include in the history.</param>
    /// <param name="periodOfTime">The maximum number of records to return. If null, all matching records are returned.</param>
    /// <returns>An IQueryable of CashDTO objects representing the user's saved cash amount history, filtered by the specified
    /// cash type(SavingAccount or PPK) and limited by the period of time if provided.</returns>
    public async Task<List<CashDTO>> GetCashHistoryAsync(int userId, CashType cashType, int? periodOfTime)
    {
        // Start with a query that filters cash records by userId and cashType
        var query = _dbContext.Cash
            .Where(c => c.UserId == userId && c.CashType == cashType);

        // If a period of time is specified
        if (periodOfTime.HasValue)
        {
            return await query
                .OrderByDescending(c => c.Date) // Order by date in descending order to get the most recent records first
                .Take(periodOfTime.Value) // Take the specified number of most recent records
                .Select(CashMapper.Projection) // Project to CashDTO
                .OrderBy(c => c.Date) // Order the final result by date in ascending order before returning
                .ToListAsync(); // Execute the query and return the results as a list
        }

        // If no period of time is specified, return all matching records ordered by date in ascending order
        return await query
            .OrderBy(c => c.Date)
            .Select(CashMapper.Projection)
            .ToListAsync();
    }

    /// <summary>
    /// Gets the most recent cash amount record for a specified user and cash type(SavingAccount or PPK). 
    /// If there are multiple records with the same date, the one with the highest amount will be returned.
    /// If there are no records for the specified user and cash type, null will be returned.
    /// </summary>
    /// <param name="userId">The unique identifier of the user whose cash record is to be retrieved.</param>
    /// <param name="cashType">The type of cash record to retrieve (SavingAccount or PPK).</param>
    /// <returns>A CashDTO representing the most recent cash record for the specified user and cash type, or null if no records are found.</returns>
    public async Task<CashDTO?> GetCurrentCashAsync(int userId, CashType cashType)
    {
        return await _dbContext.Cash
            .Where(c => c.UserId == userId && c.CashType == cashType)
            .OrderByDescending(c => c.Date)
            .ThenByDescending(c => c.Amount)
            .Select(CashMapper.Projection)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Gets a single cash amount record by its unique identifier. If there is no record with the specified identifier, null will be returned.
    /// </summary>
    /// <param name="cashId">The unique identifier of the cash record to retrieve.</param>
    /// <returns>A CashDTO representing the cash record with the specified identifier, or null if no record is found.</returns>
    public async Task<CashDTO?> GetSingleCashValueAsync(int cashId)
    {
        return await _dbContext.Cash
        .Where(c => c.Id == cashId)
        .Select(CashMapper.Projection) 
        .FirstOrDefaultAsync();
    }

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
    public async Task<int> InsertCashAsync(int userId, CreateCashDTO createCashDTO)
    {
        // 1. Validate the input data - amount must be non-negative.
        if (createCashDTO.Amount < 0) throw new ArgumentException("Amount cannot be negative");

        // 2. Normalize the date to the first day of the month
        var dateInput = createCashDTO.Date ?? DateOnly.FromDateTime(DateTime.Today);
        var normalizedDate = new DateOnly(dateInput.Year, dateInput.Month, 1);

        // 3. Validate the date - it cannot be in the future.
        if (normalizedDate > DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Cannot insert future data");

        // 4. Delete any existing record for the same user, cash type, and month before inserting the new record.
        var existingRecords = _dbContext.Cash
            .Where(c => c.UserId == userId
                     && c.CashType == createCashDTO.CashType
                     && c.Date == normalizedDate);

        _dbContext.Cash.RemoveRange(existingRecords);

        // 5. Insert the new cash record with the normalized date.
        var cashEntity = new CashEntity
        {
            UserId = userId,
            Amount = createCashDTO.Amount,
            CashType = createCashDTO.CashType,
            Date = normalizedDate
        };

        await _dbContext.Cash.AddAsync(cashEntity);
        await _dbContext.SaveChangesAsync();

        return cashEntity.Id;
    }

    /// <summary>
    /// Asynchronously deletes a cash record identified by its unique identifier. 
    /// If no record with the specified identifier exists, an ArgumentOutOfRangeException is thrown.
    /// </summary>
    /// <param name="cashId">The unique identifier of the cash record to delete.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    /// <exception cref="ArgumentException">Thrown if no cash record with the specified identifier exists.</exception>
    public async Task DeleteCashAsync(int cashId)
    {
        // ExecuteDeleteAsync sends "DELETE FROM Cash WHERE Id = @id" directly to the database
        int deletedRows = await _dbContext.Cash
            .Where(d => d.Id == cashId)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new ArgumentException($"There was no cash record with given id: {cashId}");
        }
    }
}
