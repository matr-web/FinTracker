using FinTracker.BLL.Mappers;
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

    /// <inheritdoc cref="IHistoryService.GetHistoryAsync" />
    public async Task<IEnumerable<HistoryDTO>> GetHistoryAsync(int userId)
    {
        return await _dbContext.Histories
            .Where(h => h.UserId == userId)
            .Select(HistoryMapper.Projection)
            .ToListAsync();
    }

    /// <inheritdoc cref="IHistoryService.GetSingleHistoryElementAsync" />
    public async Task<HistoryDTO?> GetSingleHistoryElementAsync(int userId, int historyId)
    {
        return await _dbContext.Histories
            .Where(h => h.Id == historyId && h.UserId == userId)
            .Select(HistoryMapper.Projection)   
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc cref="IHistoryService.InsertHistoryElementAsync" />
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
            CurrencyCode = createHistoryDTO.CurrencyCode,
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

    /// <inheritdoc cref="IHistoryService.DeleteSingleHistoryElementAsync" />
    public async Task<bool> DeleteSingleHistoryElementAsync(int userId, int historyId)
    {
        var deletedRows = await _dbContext.Histories
            .Where(h => h.Id == historyId && h.UserId == userId)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new ArgumentException($"There was no history record with given id: {historyId}");
        }

        return true;
    }

    /// <inheritdoc cref="IHistoryService.DeleteWholeHistoryAsync" />
    public async Task<int> DeleteWholeHistoryAsync(int userId)
    {
        var deletedRows = await _dbContext.Histories
            .Where(h => h.UserId == userId)
            .ExecuteDeleteAsync();

        return deletedRows;
    }
}
