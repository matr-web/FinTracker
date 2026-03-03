using FinTracker.BLL.Services.Interfaces;
using FinTracker.BLL.Utils;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HoldingDTOs;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.BLL.Services;

public class HoldingService : IHoldingService
{
    private readonly FinTrackerDbContext _dbContext;
    private readonly ICurrencyService _currencyService;
    private readonly IStockService _stockService;

    public HoldingService(FinTrackerDbContext dbContext, ICurrencyService currencyService, IStockService stockService)
    {
        _dbContext = dbContext;
        _currencyService = currencyService;
        _stockService = stockService;
    }

    public async Task<HoldingDTO?> GetHoldingAsync(int Id)
    {
        var holdingEntity = await _dbContext.Holdings
             .FirstOrDefaultAsync(h => h.Id == Id);

        if (holdingEntity == null) 
        {
            return null;
        }

        // Get the current stock data for the holding's ticker symbol.
        var stockData = await _stockService.GetStockDataAsync(holdingEntity.TickerSymbol);

        // Get exange rate.
        var exchangeRate = await _currencyService.GetExchangeRateFrankfurterAsync(holdingEntity.CurrencyCode.ToString(), "PLN");

        // Find all holdings for the user from the database.
        var holdingsFromDb = await _dbContext.Holdings
            .Where(h => h.UserId == holdingEntity.UserId)
            .ToListAsync();

        decimal totalPortfolioValueOfGivenUser = 0;

        foreach (var holding in holdingsFromDb)
        {
            var holdingData = await _stockService.GetStockDataAsync(holding.TickerSymbol);

            // Calculate total current value by multiplying the current stock price, current exchange rate, and quantity.
            totalPortfolioValueOfGivenUser += (holdingData != null ? holdingData.CurrentPrice : 0)
                * exchangeRate * holding.Quantity;
        }

        if (holdingEntity != null)
        {
            return new HoldingDTO
            {
                Id = holdingEntity.Id,
                TickerSymbol = holdingEntity.TickerSymbol,
                StockName = holdingEntity.StockName,
                Quantity = holdingEntity.Quantity,
                BuyPrice = holdingEntity.BuyPrice,
                CurrentPrice = stockData?.CurrentPrice ?? 0,
                CurrentHoldingValue = stockData?.CurrentPrice ?? 0 * 
                holdingEntity.Quantity * exchangeRate,
                CurrencyCode = holdingEntity.CurrencyCode,
                CurrencyPriceWhenBought = holdingEntity.CurrencyPrice,
                CurrentCurrencyPrice = exchangeRate,
                PercentageChange = FinanceUtils.CalculatePercentageChange(holdingEntity.BuyPrice, stockData?.CurrentPrice ?? 0),
                PercentageChangeWithCurrencyChangesCalculated =
                    FinanceUtils.CalculatePercentageChange(holdingEntity.BuyPrice * holdingEntity.CurrencyPrice, stockData?.CurrentPrice * exchangeRate ?? 0),
                PortfolioPercentage = ((stockData?.CurrentPrice ?? 0) *
                holdingEntity.Quantity * exchangeRate) / totalPortfolioValueOfGivenUser * 100,
                UserId = holdingEntity.UserId
            };
        }

        return null;
    }

    public async Task<int> InsertOrUpdateHoldingAsync(CreateHoldingDTO createHoldingDTO, int userId)
    {
        // Check if this user has already a holding with the same ticker symbol and currency code for the user.
        var currentDataOfThisHolding = await _dbContext.Holdings
            .FirstOrDefaultAsync(h => h.UserId == userId 
            && h.TickerSymbol == createHoldingDTO.TickerSymbol
            && h.CurrencyCode == createHoldingDTO.CurrencyCode);

        // Check if the CurrencyPrice is null, if it is, get the exchange rate for the given currency code to PLN
        // and set it as the CurrencyPrice.
        if (createHoldingDTO.CurrencyPrice == null)
        {
            createHoldingDTO.CurrencyPrice = await _currencyService.GetExchangeRateFrankfurterAsync(createHoldingDTO.CurrencyCode.ToString(), "PLN");
        }

        // Update the existing holding if it exists, otherwise create a new one. 
        if (currentDataOfThisHolding != null) {
            // Calculate the new average buy price based on the existing quantity and buy price, and the new quantity and buy price.
            currentDataOfThisHolding.BuyPrice = 
                ((currentDataOfThisHolding.BuyPrice * currentDataOfThisHolding.Quantity) + 
                (createHoldingDTO.BuyPrice * createHoldingDTO.Quantity)) / 
                (currentDataOfThisHolding.Quantity + createHoldingDTO.Quantity);
            // Calculate the new average currency price based on the existing quantity and currency price,
            // and the new quantity and currency price.
            currentDataOfThisHolding.CurrencyPrice =
                ((currentDataOfThisHolding.CurrencyPrice * currentDataOfThisHolding.Quantity) +
                ((decimal)createHoldingDTO.CurrencyPrice * createHoldingDTO.Quantity)) /
                (currentDataOfThisHolding.Quantity + createHoldingDTO.Quantity);

            currentDataOfThisHolding.Quantity += createHoldingDTO.Quantity;

            _dbContext.Holdings.Update(currentDataOfThisHolding);
            await _dbContext.SaveChangesAsync();
            
            return currentDataOfThisHolding.Id;
        }

        var holdingEntity = new HoldingEntity
        {
            StockName = createHoldingDTO.StockName,
            TickerSymbol = createHoldingDTO.TickerSymbol, 
            Quantity = createHoldingDTO.Quantity,
            BuyPrice = createHoldingDTO.BuyPrice,
            CurrencyCode = createHoldingDTO.CurrencyCode,
            CurrencyPrice = (decimal)createHoldingDTO.CurrencyPrice,
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
