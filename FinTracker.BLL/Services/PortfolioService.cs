using FinTracker.BLL.Mappers;
using FinTracker.BLL.Services.Interfaces;
using FinTracker.BLL.Utils;
using FinTracker.DAL.EF;
using FinTracker.Models.DTOs.HoldingDTOs;
using FinTracker.Models.DTOs.PortfolioDTOs;
using FinTracker.Models.Response;
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

    public async Task<PortfolioViewModel> GetPortfolioAsync(int userId)
    {
        // Find all holdings for the user from the database.
        var holdingsFromDb = await _dbContext.Holdings
            .Where(h => h.UserId == userId)
            .ToListAsync();

        // Find unique currency codes from the holdings
        var uniqueCurrencies = holdingsFromDb
            .Select(h => h.CurrencyCode.ToString())
            .Distinct();

        // Cache exchange rates to avoid multiple API calls for the same currency
        var ratesCache = new Dictionary<string, decimal>();
        foreach (var code in uniqueCurrencies)
        {
            ratesCache[code] = await _currencyService.GetExchangeRateFrankfurterAsync(code, "PLN");
        }

        var holdingsCache = new Dictionary<string, StockDataResponse?>();

        foreach(var holding in holdingsFromDb)
        {
            holdingsCache[holding.TickerSymbol] = await _stockService.GetStockDataAsync(holding.TickerSymbol) ?? null;
        }

        decimal totalValueInvested = 0;
        decimal totalCurrentValue = 0;

        foreach (var holding in holdingsFromDb)
        {
            // Calculate total value invested by multiplying the buy price,
            // currency price at the time of purchase, and quantity.
            totalValueInvested += holding.BuyPrice
                * holding.CurrencyPrice
                * holding.Quantity;

            // Calculate total current value by multiplying the current stock price,
            // current exchange rate, and quantity.
            totalCurrentValue += (holdingsCache[holding.TickerSymbol]?.CurrentPrice ?? 0)
                * ratesCache[holding.CurrencyCode.ToString()]
                * holding.Quantity;
        }

        var holdingsDTOs = new List<HoldingDTO>();

        foreach (var h in holdingsFromDb)
        {
            holdingsDTOs.Add(new HoldingDTO
            {
                Id = h.Id,
                TickerSymbol = h.TickerSymbol,
                StockName = h.StockName,
                Quantity = h.Quantity,
                BuyPrice = h.BuyPrice,
                CurrentPrice = holdingsCache[h.TickerSymbol]?.CurrentPrice ?? 0,
                CurrentHoldingValue = (holdingsCache[h.TickerSymbol]?.CurrentPrice ?? 0) *
                h.Quantity * ratesCache[h.CurrencyCode.ToString()],
                CurrencyCode = h.CurrencyCode,
                CurrencyPriceWhenBought = h.CurrencyPrice,
                CurrentCurrencyPrice = ratesCache[h.CurrencyCode.ToString()],
                PercentageChange = FinanceUtils.CalculatePercentageChange(h.BuyPrice, holdingsCache[h.TickerSymbol]?.CurrentPrice ?? 0),
                PercentageChangeWithCurrencyChangesCalculated =
                    FinanceUtils.CalculatePercentageChange(h.BuyPrice * h.CurrencyPrice, holdingsCache[h.TickerSymbol]?.CurrentPrice * h.CurrencyPrice ?? 0),
                PortfolioPercentage = ((holdingsCache[h.TickerSymbol]?.CurrentPrice ?? 0) *
                h.Quantity * ratesCache[h.CurrencyCode.ToString()]) / totalCurrentValue * 100,
                UserId = userId
            });
        }

        // Calculate the total percentage change for the entire portfolio based on the total value invested and the total current value.
        var totalPercentageChange = FinanceUtils.CalculatePercentageChange(totalValueInvested, totalCurrentValue);

        // Return the portfolio view model with the total value invested, total current value, total percentage change, and the list of holdings.
        var portfolio = new PortfolioViewModel()
        {
            ValueInvested = totalValueInvested,
            TotalValue = totalCurrentValue,
            TotalPercentageChange = totalPercentageChange,
            Holdings = holdingsDTOs
        };

        return portfolio;
    }

    public IQueryable<PortfolioDTO> GetPortfolioHistory(int userId)
    {
        return _dbContext.PortfolioEntities
            .Include(p => p.User)
            .Where(p => p.UserId == userId)
            .OrderBy(p => p.Date)
            .Select(PortfolioMapper.Projection);
    }

    public async Task<PortfolioDTO?> GetPortfolioSaveByIdAsync(int portfolioId)
    {
        return await _dbContext.PortfolioEntities
          .Include(p => p.User)
          .Where(p => p.Id == portfolioId)
          .Select(PortfolioMapper.Projection)
          .FirstOrDefaultAsync();
    }

    public async Task<int> SaveCurrentPortfolioDataAsync(int userId)
    {
        var portfolioViewModel = await GetPortfolioAsync(userId);

        if (portfolioViewModel == null)
        {
            throw new Exception("Unable to retrieve portfolio data for saving.");
        }

        DateTime today = DateTime.Today;

        // Find if there is already a portfolio saved for the user with the same date (month and year).
        var portfolioWithTheSameDate = _dbContext.PortfolioEntities.Where(p => p.UserId == userId 
        && p.Date == DateOnly.FromDateTime(new DateTime(today.Year, today.Month, 1))).ToList();

        //  If there are any, delete them, because we want to have only one portfolio value for each month.
        if (portfolioWithTheSameDate != null && portfolioWithTheSameDate.Count() > 0)
        {
            _dbContext.PortfolioEntities.RemoveRange(portfolioWithTheSameDate);
        }       

        var portfolioEntity = new DAL.Entities.PortfolioEntity()
        {
            UserId = userId,
            Date = DateOnly.FromDateTime(new DateTime(today.Year, today.Month, 1)),
            ValueInvested = portfolioViewModel.ValueInvested,
            TotalValue = portfolioViewModel.TotalValue,
            TotalPercentageChange = portfolioViewModel.TotalPercentageChange
        };

        await _dbContext.PortfolioEntities.AddAsync(portfolioEntity);
        await _dbContext.SaveChangesAsync();

        return portfolioEntity.Id;
    }

    public async Task<int> SaveHistoricalPortfolioDataAsync(int userId, SavePortfolioDTO savePortfolioDTO)
    {
        // Check if the given date is not from the future.
        if (savePortfolioDTO.Date > DateOnly.FromDateTime(DateTime.Today))
        {
            throw new ArgumentOutOfRangeException(
                    paramName: nameof(savePortfolioDTO.Date),
                    actualValue: savePortfolioDTO.Date,
                    $"The given date: {savePortfolioDTO.Date} is from the future. Today date: " +
                    $"{DateTime.Today}."
                    );
        }

        var portfolioEntity = new DAL.Entities.PortfolioEntity()
        {
            Date = DateOnly.FromDateTime(new DateTime(savePortfolioDTO.Date.Year, savePortfolioDTO.Date.Month, 1)),
            ValueInvested = savePortfolioDTO.ValueInvested,
            TotalValue = savePortfolioDTO.TotalValue,
            TotalPercentageChange = savePortfolioDTO.TotalPercentageChange,
            UserId = userId
        };
        await _dbContext.PortfolioEntities.AddAsync(portfolioEntity);
        await _dbContext.SaveChangesAsync();
        return portfolioEntity.Id;
    }
}