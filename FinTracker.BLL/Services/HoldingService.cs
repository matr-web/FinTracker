using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HoldingDTOs;

namespace FinTracker.BLL.Services;

public class HoldingService : IHoldingService
{
    private readonly FinTrackerDbContext _dbContext;

    public HoldingService(FinTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<HoldingDTO> GetHoldingsAsync(int userId)
    {
        return _dbContext.Holdings
             .Where(h => h.UserId == userId)
             .Select(h => new HoldingDTO
             {
                 Id = h.UserId,
                 StockName = h.StockName,
                 Value = h.Value,
                 UserId = userId
             });
    }

    public async Task<int> InsertHoldingAsync(CreateHoldingDTO createHoldingDTO, int userId)
    {
        var holdingEntity = new HoldingEntity
        {
            StockName = createHoldingDTO.StockName,
            Value = createHoldingDTO.Value,
            UserId = userId
        };

        await _dbContext.Holdings.AddAsync(holdingEntity);
        await _dbContext.SaveChangesAsync();

        return holdingEntity.Id;
    }

    public async Task DeleteHoldingAsync(int holdingId)
    {
        var obj = _dbContext.Holdings.SingleOrDefault(h => h.Id == holdingId);

        if (obj != null)
        {
            _dbContext.Holdings.Remove(obj);
            await _dbContext.SaveChangesAsync();
        }
    }
}
