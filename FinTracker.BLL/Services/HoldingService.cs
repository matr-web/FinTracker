using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HoldingDTOs;
using FinTracker.Models.Response;
using FinTracker.Models.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Net.Http.Json;
using YahooFinanceApi;

namespace FinTracker.BLL.Services;

public class HoldingService : IHoldingService
{
    private readonly FinTrackerDbContext _dbContext;

    public HoldingService(FinTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PortfolioViewModel> GetPortfolioAsync(int userId)
    {
        // Find all holdings for the user from the database.
        var holdingsFromDb = await _dbContext.Holdings
            .Where(h => h.UserId == userId)
            .ToListAsync();

        // Find unique currency codes from the holdings
        var uniqueCurrencies = holdingsFromDb.Select(h => h.CurrencyCode.ToString()).Distinct();

        // Cache exchange rates to avoid multiple API calls for the same currency
        var ratesCache = new Dictionary<string, decimal>();
        foreach (var code in uniqueCurrencies)
        {
            ratesCache[code] = await GetExchangeRateFrankfurter(code, "PLN");
        }

        decimal totalValueInvested = 0;
        decimal totalCurrentValue = 0;

        foreach (var holding in holdingsFromDb)
        {
            // Calculate total value invested by multiplying the buy price, currency price at the time of purchase, and quantity.
            totalValueInvested += holding.BuyPrice * holding.CurrencyPrice * holding.Quantity;

            // Calculate total current value by multiplying the current stock price, current exchange rate, and quantity.
            totalCurrentValue += ((await GetStockData(holding.TickerSymbol))?.CurrentPrice ?? 0)
                * await GetExchangeRateFrankfurter(holding.CurrencyCode.ToString(), "PLN") * holding.Quantity;
        }

        // Calculate the total percentage change for the entire portfolio based on the total value invested and the total current value.
        var totalPercentageChange = CalculatePercentageChange(totalValueInvested, totalCurrentValue);

        var holdingsDTOs = new List<HoldingDTO>();  

        foreach (var h in holdingsFromDb)
        {
            // Get the current stock data for the holding's ticker symbol.
            var stockData = await GetStockData(h.TickerSymbol);
            // Get the exchange rate for the holding's currency code from the cache.
            var exchangeRate = ratesCache[h.CurrencyCode.ToString()];

            holdingsDTOs.Add(new HoldingDTO {
                Id = h.Id,
                TickerSymbol = h.TickerSymbol,
                StockName = h.StockName,
                Quantity = h.Quantity,
                BuyPrice = h.BuyPrice,
                CurrentPrice = stockData ?.CurrentPrice ?? 0,
                CurrentHoldingValue = (stockData?.CurrentPrice ?? 0) *
                h.Quantity * exchangeRate,
                CurrencyCode = h.CurrencyCode,
                CurrencyPriceWhenBought = h.CurrencyPrice,
                CurrentCurrencyPrice = exchangeRate,
                PercentageChange = CalculatePercentageChange(h.BuyPrice, stockData?.CurrentPrice ?? 0),
                PercentageChangeWithCurrencyChangesCalculated =
                    CalculatePercentageChange(h.BuyPrice * h.CurrencyPrice, stockData?.CurrentPrice * h.CurrencyPrice ?? 0),
                PortfolioPercentage = ((stockData?.CurrentPrice ?? 0) *
                h.Quantity * exchangeRate) / totalCurrentValue * 100,
                UserId = userId
            });
        }

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

    public async Task<HoldingDTO?> GetHoldingAsync(int Id)
    {
        var holdingEntity = await _dbContext.Holdings
             .FirstOrDefaultAsync(h => h.Id == Id);

        if (holdingEntity == null) 
        {
            return null;
        }

        // Get the current stock data for the holding's ticker symbol.
        var stockData = await GetStockData(holdingEntity.TickerSymbol);

        // Get exange rate.
        var exchangeRate = await GetExchangeRateFrankfurter(holdingEntity.CurrencyCode.ToString(), "PLN");

        // Find all holdings for the user from the database.
        var holdingsFromDb = await _dbContext.Holdings
            .Where(h => h.UserId == holdingEntity.UserId)
            .ToListAsync();

        decimal totalCurrentValue = 0;

        foreach (var holding in holdingsFromDb)
        {
            // Calculate total current value by multiplying the current stock price, current exchange rate, and quantity.
            totalCurrentValue += ((await GetStockData(holding.TickerSymbol))?.CurrentPrice ?? 0)
                * await GetExchangeRateFrankfurter(holding.CurrencyCode.ToString(), "PLN") * holding.Quantity;
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
                PercentageChange = CalculatePercentageChange(holdingEntity.BuyPrice, stockData?.CurrentPrice ?? 0),
                PercentageChangeWithCurrencyChangesCalculated =
                    CalculatePercentageChange(holdingEntity.BuyPrice * holdingEntity.CurrencyPrice, stockData?.CurrentPrice * exchangeRate ?? 0),
                PortfolioPercentage = ((stockData?.CurrentPrice ?? 0) *
                holdingEntity.Quantity * exchangeRate) / totalCurrentValue * 100,
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
            createHoldingDTO.CurrencyPrice = await GetExchangeRateFrankfurter(createHoldingDTO.CurrencyCode.ToString(), "PLN");
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

    private async Task<decimal> GetExchangeRateFrankfurter(string fromCurrency, string toCurrency)
    {
        using HttpClient client = new HttpClient();

        // URL for Frankfurter: https://api.frankfurter.app/latest?from=USD&to=PLN
        string url = $"https://api.frankfurter.app/latest?from={fromCurrency}&to={toCurrency}";

        try
        {
            var response = await client.GetFromJsonAsync<FrankfurterResponse>(url);

            if (response != null && response.Rates.ContainsKey(toCurrency))
            {
                return response.Rates[toCurrency];
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd: {ex.Message}");
        }

        return 1m; // Return 1 if there's an error or if the rate is not found.
    }

    private async Task<StockDataResponse?> GetStockData(string symbol)
    {
        try
        {
            // Zapytanie do Yahoo Finance o konkretne pola
            var query = await Yahoo.Symbols(symbol)
                                   .Fields(Field.RegularMarketPrice,
                                           Field.Currency,
                                           Field.LongName,
                                           Field.RegularMarketTime)
                                   .QueryAsync();

            if (!query.ContainsKey(symbol))
            {
                return null; // Lub rzuć wyjątek, jeśli symbol jest błędny
            }

            var ticker = query[symbol];

            // Mapujemy wynik na naszą klasę StockData
            return new StockDataResponse
            {
                Symbol = symbol,
                Name = ticker.LongName,
                CurrentPrice = (decimal)ticker.RegularMarketPrice,
                Currency = ticker.Currency,
                // Przeliczamy czas z formatu Unix na DateTime
                LastTradeTime = DateTimeOffset.FromUnixTimeSeconds(ticker.RegularMarketTime).DateTime
            };
        }
        catch (Exception ex)
        {
            // Logowanie błędu (opcjonalnie)
            throw new Exception($"Nie udało się pobrać danych dla {symbol}: {ex.Message}");
        }
    }

    // Calculate the percentage change between the buy price and the current price.
    private decimal CalculatePercentageChange(decimal buyPrice, decimal currentPrice)
    {
        if (buyPrice == 0) return 0; // Avoid division by zero
        return ((currentPrice - buyPrice) / buyPrice) * 100;
    }
}
