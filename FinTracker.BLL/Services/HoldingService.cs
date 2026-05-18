using FinTracker.BLL.Mappers;
using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HoldingDTOs;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.BLL.Services;

public class HoldingService : IHoldingService
{
    private readonly FinTrackerDbContext _dbContext;
    private readonly ICurrencyService _currencyService;

    public HoldingService(FinTrackerDbContext dbContext, ICurrencyService currencyService)
    {
        _dbContext = dbContext;
        _currencyService = currencyService;
    }

    /// <inheritdoc cref="IHoldingService.GetHoldingByIdAsync" />
    public async Task<HoldingDTO?> GetHoldingByIdAsync(int userId, int holdingId)
    {
        return await _dbContext.Holdings
            .Where(h => h.UserId == userId && h.Id == holdingId)
            .Select(HoldingMapper.Projection)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc cref="IHoldingService.InsertOrUpdateHoldingAsync" />
    public async Task<int> InsertOrUpdateHoldingAsync(CreateHoldingDTO createHoldingDTO, int userId)
    {
        // 1. Check if the CurrencyPrice is null, if it is, get the exchange rate for the given currency code to PLN.
        var currentCurrencyPrice = createHoldingDTO.CurrencyPrice ??
            await _currencyService.GetExchangeRateAsync(createHoldingDTO.CurrencyCode.ToString(), "PLN");

        // 2. Check if this user has already a holding with the same ticker symbol and currency code for the user.
        var holding = await _dbContext.Holdings
            .FirstOrDefaultAsync(h => h.UserId == userId 
            && h.TickerSymbol == createHoldingDTO.TickerSymbol
            && h.CurrencyCode == createHoldingDTO.CurrencyCode);

        // 3. Update the existing holding if it exists
        if (holding != null)
        {
            // 3.1 Calculate the new average buy price based on the existing quantity and buy price, and the new quantity and buy price.
            holding.BuyPrice = 
                ((holding.BuyPrice * holding.Quantity) + 
                (createHoldingDTO.BuyPrice * createHoldingDTO.Quantity)) / 
                (holding.Quantity + createHoldingDTO.Quantity);
            // 3.2 Calculate the new average currency price based on the existing quantity,
            // currency price, the current added quantity and current currency price.
            holding.CurrencyPrice =
                ((holding.CurrencyPrice * holding.Quantity) +
                (currentCurrencyPrice * createHoldingDTO.Quantity)) /
                (holding.Quantity + createHoldingDTO.Quantity);

            // 3.3 Update the quantity by adding the new quantity to the existing quantity.
            holding.Quantity += createHoldingDTO.Quantity;  
        }
        // 4. Insert a new holding if it does not exist.
        else
        {
            // 4.1 Create a new holding entity and set its properties based on the provided DTO and the current currency price.
            holding = new HoldingEntity
            {
                StockName = createHoldingDTO.StockName,
                TickerSymbol = createHoldingDTO.TickerSymbol,
                Quantity = createHoldingDTO.Quantity,
                BuyPrice = createHoldingDTO.BuyPrice,
                CurrencyCode = createHoldingDTO.CurrencyCode,
                CurrencyPrice = currentCurrencyPrice,
                UserId = userId
            };

            await _dbContext.Holdings.AddAsync(holding);
        }

        // 5. Save the changes to the database and return the id of the inserted or updated holding.
        await _dbContext.SaveChangesAsync();

        return holding.Id;
    }

    /// <inheritdoc cref="IHoldingService.DeleteHoldingAsync" />
    public async Task<bool> DeleteHoldingAsync(int userId, int holdingId)
    {
        var deletedRows = await _dbContext.Holdings
            .Where(h => h.Id == holdingId && h.UserId == userId)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new ArgumentException($"There was no holding record with given id: {holdingId}");
        }

        return true;
    }
}
