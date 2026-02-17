using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HoldingDTOs;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.BLL.Services;

public class HoldingService : IHoldingService
{
    private readonly FinTrackerDbContext _dbContext;

    public HoldingService(FinTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<HoldingDTO> GetHoldings(int userId)
    {
        return _dbContext.Holdings
             .Where(h => h.UserId == userId)
             .Select(h => new HoldingDTO
             {
                 Id = h.UserId,
                 TickerSymbol = h.TickerSymbol,
                 StockName = h.StockName,
                 Quantity = h.Quantity,
                 BuyPrice = h.BuyPrice,
                 Currency = h.Currency,
                 UserId = userId
             });
    }

    public async Task<HoldingDTO?> GetHoldingAsync(int Id)
    {
        var holdingEntity = await _dbContext.Holdings
             .FirstOrDefaultAsync(h => h.Id == Id);

        if(holdingEntity != null)
        {
            return new HoldingDTO
            {
                Id = holdingEntity.Id,
                TickerSymbol = holdingEntity.TickerSymbol,
                StockName = holdingEntity.StockName,
                Quantity = holdingEntity.Quantity,
                BuyPrice = holdingEntity.BuyPrice,
                Currency = holdingEntity.Currency,
                UserId = holdingEntity.UserId
            };
        }

        return null;
    }

    public async Task<int> InsertHoldingAsync(CreateHoldingDTO createHoldingDTO, int userId)
    {
        var holdingEntity = new HoldingEntity
        {
            StockName = createHoldingDTO.StockName,
            TickerSymbol = createHoldingDTO.TickerSymbol, 
            Quantity = createHoldingDTO.Quantity,
            BuyPrice = createHoldingDTO.BuyPrice,
            Currency= createHoldingDTO.Currency,
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
