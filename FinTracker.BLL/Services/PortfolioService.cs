using FinTracker.BLL.Mappers;
using FinTracker.BLL.Services.Interfaces;
using FinTracker.BLL.Utils;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.PortfolioDTOs;
using FinTracker.Models.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.BLL.Services;

public class PortfolioService : IPortfolioService
{
    private readonly FinTrackerDbContext _dbContext;
    private readonly ICurrencyService _currencyService;
    private readonly IStockService _stockService;

    public PortfolioService(FinTrackerDbContext dbContext, ICurrencyService currencyService, IStockService stockService)
    {
        _dbContext = dbContext;
        _currencyService = currencyService;
        _stockService = stockService;
    }

    /// <inheritdoc cref="IPortfolioService.GetCurrentPortfolioDataAsync" />
    public async Task<PortfolioViewModel> GetCurrentPortfolioDataAsync(int userId)
    {
        // 1. Find all holdings for the user from the database.
        var holdingsFromDb = await _dbContext.Holdings
            .Where(h => h.UserId == userId)
            .ToListAsync();

        // 2. For each unique currency code in the holdings, fetch the current exchange rate to PLN and cache it.
        var uniqueCurrencies = holdingsFromDb
            .Select(h => h.CurrencyCode.ToString())
            .Distinct();

        var currencyTasks = uniqueCurrencies.Select(async code =>
        {
            var rate = await _currencyService.GetExchangeRateAsync(code, "PLN");
            return new { Code = code, Rate = rate };
        });
        var ratesCache = (await Task.WhenAll(currencyTasks)).ToDictionary(x => x.Code, x => x.Rate);

        // 3. For each unique ticker symbol in the holdings, fetch the current stock price and cache it.
        var uniqueTickers = holdingsFromDb.Select(h => h.TickerSymbol).Distinct();
        var stockTasks = uniqueTickers.Select(async ticker =>
        {
            var data = await _stockService.GetStockDataAsync(ticker);
            return new { Ticker = ticker, Data = data };
        });
        var holdingsCache = (await Task.WhenAll(stockTasks)).ToDictionary(x => x.Ticker, x => x.Data);

        // 4. Calculate the total value invested, total current value and 
        decimal totalValueInvested = 0;
        decimal totalCurrentValue = 0;

        foreach (var holding in holdingsFromDb)
        {
            // Calculate total value invested by multiplying the buy price,
            // currency price at the time of purchase and quantity.
            totalValueInvested += holding.BuyPrice
                * holding.CurrencyPrice
                * holding.Quantity;

            // Calculate total current value by multiplying the current stock price,
            // current exchange rate and quantity.
            totalCurrentValue += (holdingsCache[holding.TickerSymbol]?.CurrentPrice ?? 0)
                * ratesCache[holding.CurrencyCode.ToString()]
                * holding.Quantity;
        }

        // 5.Calculate the total percentage change for the entire portfolio based on the total value invested and the total current value.
        var totalPercentageChange = FinanceUtils.CalculatePercentageChange(totalValueInvested, totalCurrentValue);

        // 6. For each holding, calculate the current price, current holding value, percentage change,
        // percentage change with currency changes calculated and portfolio percentage, and create a PortfolioHoldingDTO for it.
        var holdingsDTOs = new List<PortfolioHoldingDTO>();

        foreach (var h in holdingsFromDb)
        {
            var currentPrice = holdingsCache[h.TickerSymbol]?.CurrentPrice ?? 0;
            var exchangeRate = ratesCache[h.CurrencyCode.ToString()];
            var currentHoldingValue = currentPrice * h.Quantity * exchangeRate;
            var originalHoldingValue = h.BuyPrice * h.CurrencyPrice * h.Quantity;
            var percentageChange = FinanceUtils.CalculatePercentageChange(h.BuyPrice, currentPrice);
            var percentageChangeWithCurrencyChangesCalculated = FinanceUtils.CalculatePercentageChange(originalHoldingValue, currentHoldingValue);
            var portfolioPercentage = totalCurrentValue == 0 ? 0 : (currentPrice * h.Quantity * exchangeRate) / totalCurrentValue * 100;

            holdingsDTOs.Add(new PortfolioHoldingDTO
            {
                Id = h.Id,
                TickerSymbol = h.TickerSymbol,
                StockName = h.StockName,
                Quantity = h.Quantity,
                BuyPrice = h.BuyPrice,
                CurrentPrice = currentPrice,
                CurrentHoldingValue = currentHoldingValue,
                CurrencyCode = h.CurrencyCode,
                CurrencyPriceWhenBought = h.CurrencyPrice,
                CurrentCurrencyPrice = exchangeRate,
                PercentageChange = percentageChange,
                PercentageChangeWithCurrencyChangesCalculated = percentageChangeWithCurrencyChangesCalculated,
                PortfolioPercentage = portfolioPercentage,
                UserId = userId
            });
        }

        // 7. Return the portfolio view model with the total value invested, total current value, total percentage change, and the list of holdings.
        var portfolio = new PortfolioViewModel()
        {
            ValueInvested = totalValueInvested,
            CurrentValue = totalCurrentValue,
            TotalPercentageChange = totalPercentageChange,
            Holdings = holdingsDTOs
        };

        return portfolio;
    }

    /// <inheritdoc cref="IPortfolioService.GetPortfolioHistory" />
    public async Task<IEnumerable<PortfolioDTO>> GetPortfolioHistoryAsync(int userId)
    {
        return await _dbContext.PortfolioEntities
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Date)
            .Select(PortfolioMapper.Projection)
            .ToListAsync();
    }

    /// <inheritdoc cref="IPortfolioService.GetPortfolioSaveByIdAsync" />
    public async Task<PortfolioDTO?> GetPortfolioSaveByIdAsync(int portfolioId)
    {
        return await _dbContext.PortfolioEntities
          .Where(p => p.Id == portfolioId)
          .Select(PortfolioMapper.Projection)
          .FirstOrDefaultAsync();
    }

    /// <inheritdoc cref="IPortfolioService.SaveCurrentPortfolioDataAsync" />
    public async Task<int> SaveCurrentPortfolioDataAsync(int userId)
    {
        // 1. Retrieve the current portfolio data for the user.
        var portfolioViewModel = await GetCurrentPortfolioDataAsync(userId);

        if (portfolioViewModel == null)
        {
            throw new ArgumentException("Unable to retrieve portfolio data for saving.");
        }

        // 2. Determine the first day of the current month.
        var firstDayOfMonth = DateOnly.FromDateTime(DateTime.Today).ToFirstDayOfMonth();

        // 3. Check if there is already a portfolio save for the user for the current month in the database. If there is, delete it.
        await _dbContext.PortfolioEntities
            .Where(p => p.UserId == userId && p.Date == firstDayOfMonth)
            .ExecuteDeleteAsync();

        // 4. Create a new PortfolioEntity with the retrieved portfolio data, the user ID and the first day of the month as the date,
        // and save it to the database.
        var portfolioEntity = new PortfolioEntity()
        {
            UserId = userId,
            Date = firstDayOfMonth,
            ValueInvested = portfolioViewModel.ValueInvested,
            TotalValue = portfolioViewModel.CurrentValue,
            TotalPercentageChange = portfolioViewModel.TotalPercentageChange
        };

        await _dbContext.PortfolioEntities.AddAsync(portfolioEntity);
        await _dbContext.SaveChangesAsync();

        // 5. Return the ID of the saved portfolio.
        return portfolioEntity.Id;
    }

    /// <inheritdoc cref="IPortfolioService.SaveHistoricalPortfolioDataAsync" />
    public async Task<int> SaveHistoricalPortfolioDataAsync(int userId, SavePortfolioDTO savePortfolioDTO)
    {
        // 1. Check if the given date is not from the future.
        if (savePortfolioDTO.Date > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentOutOfRangeException(
                    paramName: nameof(savePortfolioDTO.Date),
                    actualValue: savePortfolioDTO.Date,
                    $"The given date: {savePortfolioDTO.Date} is from the future. Todays date: " +
                    $"{DateTime.Today}."
                    );
        }

        // 2. Determine the first day of the given month.
        var firstDayOfMonth = savePortfolioDTO.Date.ToFirstDayOfMonth();

        // 3. Check if there is already a portfolio save for the user for the month of the given date in the database. If there is, delete it.
        await _dbContext.PortfolioEntities
           .Where(p => p.UserId == userId && p.Date == firstDayOfMonth)
           .ExecuteDeleteAsync();

        // 4. Create a new PortfolioEntity with the given portfolio data, the user ID and the first day of the month of the given date as the date.
        var portfolioEntity = new PortfolioEntity()
        {
            Date = firstDayOfMonth,
            ValueInvested = savePortfolioDTO.ValueInvested,
            TotalValue = savePortfolioDTO.TotalValue,
            TotalPercentageChange = savePortfolioDTO.TotalPercentageChange,
            UserId = userId
        };
        await _dbContext.PortfolioEntities.AddAsync(portfolioEntity);
        await _dbContext.SaveChangesAsync();

        // 5. Return the ID of the saved portfolio.
        return portfolioEntity.Id;
    }

    /// <inheritdoc cref="IPortfolioService.DeleteSinglePortfolioAsync" />
    public async Task<bool> DeleteSinglePortfolioAsync(int userId, int portfolioId)
    {
        // ExecuteDeleteAsync sends "DELETE FROM Portfolio WHERE Id = @id AND UserId = @userId" directly to the database
        int deletedRows = await _dbContext.PortfolioEntities
            .Where(p => p.Id == portfolioId && p.UserId == userId)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new ArgumentException($"There was no portfolio record with given id: {portfolioId}");
        }

        return true;
    }

    /// <inheritdoc cref="IPortfolioService.DeleteWholePortfolioAsync" />
    public async Task<int> DeleteWholePortfolioAsync(int userId)
    {
        // ExecuteDeleteAsync sends "DELETE FROM Portfolio WHERE UserId = @userId" directly to the database
        int deletedRows = await _dbContext.PortfolioEntities
            .Where(p => p.UserId == userId)
            .ExecuteDeleteAsync();

        return deletedRows;
    }
}