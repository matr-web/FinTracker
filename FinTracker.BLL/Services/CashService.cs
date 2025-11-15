using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.CashDTOs;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.BLL.Services;

public class CashService : ICashService
{
    private readonly FinTrackerDbContext _dbContext;
    public CashService(FinTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IQueryable<CashDTO?> GetCashHistory(int userId)
    {
        return _dbContext.Cash
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .Select(CashDTO.Projection);
    }

    public async Task<CashDTO?> GetCurrentCashAsync(int userId)
    {
        return await _dbContext.Cash
            .Include(c => c.User)
            .Where(c => c.UserId == userId)
            .OrderByDescending(c => c.Date)
            .Select(CashDTO.Projection)
            .FirstOrDefaultAsync();
    }

    public async Task<CashDTO?> GetSingleCashValueAsync(int cashId)
    {
        return await _dbContext.Cash
            .Include(c => c.User)
            .Where(c => c.Id == cashId)
            .Select(CashDTO.Projection)
            .FirstOrDefaultAsync(); 
    }

    public async Task<int> InsertCashAsync(CreateCashDTO createCashDTO, int userId)
    {
        // Check if the given date is not from the future.
        if(createCashDTO.Date != null && createCashDTO.Date > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentOutOfRangeException(
                    paramName: nameof(createCashDTO.Date),
                    actualValue: createCashDTO.Date,
                    $"The given date: {createCashDTO.Date} is from the future. Today date: " +
                    $"{DateTime.Today}."
                    );
        }

        var cashEntity = new CashEntity()
        {
            UserId = userId,
            Amount = createCashDTO.Amount,
            Date = createCashDTO.Date != null ? createCashDTO.Date.Value : DateOnly.FromDateTime(DateTime.Today)
        };

        await _dbContext.Cash.AddAsync(cashEntity);
        await _dbContext.SaveChangesAsync();

        return cashEntity.Id;
    }

    public async Task DeleteCashAsync(int cashId)
    {
        var obj = _dbContext.Cash.SingleOrDefault(d => d.Id == cashId);

        if (obj == null)
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(cashId),
                actualValue: cashId,
                $"There was no cash data with given Id: {cashId}");
        }

        _dbContext.Cash.Remove(obj);
        await _dbContext.SaveChangesAsync();
    }
}
