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

    /// <inheritdoc cref="ICashService.GetCashHistoryAsync" />
    public async Task<IEnumerable<CashDTO>> GetCashHistoryAsync(int userId, CashType cashType, int? periodOfTime)
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

    /// <inheritdoc cref="ICashService.GetCurrentCashAsync" />
    public async Task<CashDTO?> GetCurrentCashAsync(int userId, CashType cashType)
    {
        return await _dbContext.Cash
            .Where(c => c.UserId == userId && c.CashType == cashType)
            .OrderByDescending(c => c.Date)
            .ThenByDescending(c => c.Amount)
            .Select(CashMapper.Projection)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc cref="ICashService.GetSingleCashValueAsync" />
    public async Task<CashDTO?> GetSingleCashValueAsync(int userId, int cashId)
    {
        return await _dbContext.Cash
        .Where(c => c.Id == cashId && c.UserId == userId)
        .Select(CashMapper.Projection) 
        .FirstOrDefaultAsync();
    }

    /// <inheritdoc cref="ICashService.InsertCashAsync" />
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

    /// <inheritdoc cref="ICashService.DeleteCashAsync" />
    public async Task<bool> DeleteCashAsync(int userId, int cashId)
    {
        // ExecuteDeleteAsync sends "DELETE FROM Cash WHERE Id = @id AND UserId = @userId" directly to the database
        int deletedRows = await _dbContext.Cash
            .Where(d => d.Id == cashId && d.UserId == userId)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new ArgumentException($"There was no cash record with given id: {cashId}");
        }

        return true;
    }
}
