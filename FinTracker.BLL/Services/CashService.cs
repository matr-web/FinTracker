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

    public IQueryable<CashDTO?> GetCashHistory(int userId, CashType cashType)
    {
        return _dbContext.Cash
            .Include(c => c.User)
            .Where(c => c.UserId == userId && c.CashType == cashType)
            .Select(CashMapper.Projection);
    }

    public async Task<CashDTO?> GetCurrentCashAsync(int userId, CashType cashType)
    {
        return await _dbContext.Cash
            .Include(c => c.User)
            .Where(c => c.UserId == userId && c.CashType == cashType)
            .OrderByDescending(c => c.Date)
            .Select(CashMapper.Projection)
            .FirstOrDefaultAsync();
    }

    public async Task<CashDTO?> GetSingleCashValueAsync(int cashId)
    {
        return await _dbContext.Cash
            .Include(c => c.User)
            .Where(c => c.Id == cashId)
            .Select(CashMapper.Projection)
            .FirstOrDefaultAsync(); 
    }

    public async Task<int> InsertCashAsync(int userId, CreateCashDTO createCashDTO)
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

        // Check if the given amount is not negative.
        if(createCashDTO.Amount < 0)
        {
            throw new ArgumentOutOfRangeException(
                    paramName: nameof(createCashDTO.Amount),
                    actualValue: createCashDTO.Amount,
                    $"The given amount: {createCashDTO.Amount} is negative. Amount should be positive."
                    );
        }

        // Search for cash values with the same date.
        var cashValuesWithTheSameDate = _dbContext.Cash
            .Where(c => c.UserId == userId 
            && c.CashType == createCashDTO.CashType
            && c.Date == createCashDTO.Date);

        //  If there are any, delete them, because we want to have only one cash value for each date.
        if (cashValuesWithTheSameDate != null)
        { 
            _dbContext.Cash.RemoveRange(cashValuesWithTheSameDate); 
        }

        var cashEntity = new CashEntity()
        {
            UserId = userId,
            Amount = createCashDTO.Amount,
            CashType = createCashDTO.CashType,
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
