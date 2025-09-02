using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HistoryDTOs;

namespace FinTracker.BLL.Services;

public class HistoryService : IHistoryService
{
    private readonly FinTrackerDbContext _dbContext;

    public HistoryService(FinTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<HistoryDTO> GetUserHistory(int userId)
    {
        return _dbContext.Histories
            .Where(h => h.UserId == userId)
            .Select(h => new HistoryDTO
            {
                Id = h.UserId,
                AssetName = h.AssetName,
                Ticker = h.Ticker,
                AssetType = h.AssetType,
                Operation = h.Operation,
                Quantity = h.Quantity,
                Value = h.Value,
                Currency = h.Currency,
                CurrencyPrice = h.CurrencyPrice,
                Description = h.Description,
                Date = h.Date,
                Profit = h.Profit,
                UserId = userId
            });
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
            Value = createHistoryDTO.Value,
            Currency = createHistoryDTO.Currency,
            CurrencyPrice = createHistoryDTO.CurrencyPrice,
            Description = createHistoryDTO.Description,
            Date = createHistoryDTO.Date,
            Profit = createHistoryDTO.Profit,
            UserId = userId
        };

        if (createHistoryDTO.ROI != 0)
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
