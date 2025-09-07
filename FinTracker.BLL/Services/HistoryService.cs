using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HistoryDTOs;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.BLL.Services;

public class HistoryService : IHistoryService
{
    private readonly FinTrackerDbContext _dbContext;

    public HistoryService(FinTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<HistoryDTO> GetHistory(int userId)
    {
        return _dbContext.Histories
            .Where(h => h.UserId == userId)
            .Select(h => new HistoryDTO
            {
                Id = h.Id,
                AssetName = h.AssetName,
                Ticker = h.Ticker,
                AssetType = h.AssetType,
                Operation = h.Operation,
                Quantity = h.Quantity,
                PricePerUnit = h.PricePerUnit,
                Currency = h.Currency,
                CurrencyPrice = h.CurrencyPrice,
                Description = h.Description,
                Date = h.Date,
                Profit = h.Profit,
                UserId = userId
            });
    }

    public async Task<HistoryDTO?> GetHistoryElementAsync(int historyId)
    {
        var userEntity = await _dbContext.Histories
            .FirstOrDefaultAsync(h => h.Id == historyId);

        if (userEntity != null)
        {
            return new HistoryDTO
            {
                Id = userEntity.Id,
                AssetName = userEntity.AssetName,
                Ticker = userEntity.Ticker,
                AssetType = userEntity.AssetType,
                Operation = userEntity.Operation,
                Quantity = userEntity.Quantity,
                PricePerUnit = userEntity.PricePerUnit,
                Currency = userEntity.Currency,
                CurrencyPrice = userEntity.CurrencyPrice,
                Description = userEntity.Description,
                Date = userEntity.Date,
                Profit = userEntity.Profit,
                UserId = userEntity.UserId
            };
        }

        return null;
    }

    public async Task<int> InsertHistoryElementAsync(CreateHistoryDTO createHistoryDTO, int userId)
    {
        var historyEntity = new HistoryEntity
        {
            AssetName = createHistoryDTO.AssetName,
            Ticker = createHistoryDTO.Ticker,
            AssetType = createHistoryDTO.AssetType,
            Operation = createHistoryDTO.Operation, 
            Quantity = createHistoryDTO.Quantity,
            PricePerUnit = createHistoryDTO.PricePerUnit,
            Currency = createHistoryDTO.Currency,
            CurrencyPrice = createHistoryDTO.CurrencyPrice,
            Description = createHistoryDTO.Description,
            Date = createHistoryDTO.Date,
            Profit = createHistoryDTO.Profit,
            UserId = userId
        };

        if (createHistoryDTO.ROI != null && createHistoryDTO.ROI != 0)
        {
            historyEntity.ROIBps = (int)(createHistoryDTO.ROI * 100m);
        }

        await _dbContext.Histories.AddAsync(historyEntity);
        await _dbContext.SaveChangesAsync();

        return historyEntity.Id;
    }

    public async Task DeleteSingleHistoryElementAsync(int historyId)
    {
        var obj = _dbContext.Histories.SingleOrDefault(h => h.Id == historyId);

        if(obj != null)
        {
            _dbContext.Histories.Remove(obj);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteWholeHistoryAsync(int userId)
    {
        var objects = _dbContext.Histories.Where(h => h.UserId == userId).ToList(); 

        if(objects.Count > 0)
        {
            _dbContext.Histories.RemoveRange(objects);
            await _dbContext.SaveChangesAsync();
        }
    }
}
