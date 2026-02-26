using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.HoldingDTOs;
using FinTracker.Models.Response;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

namespace FinTracker.BLL.Services;

public class HoldingService : IHoldingService
{
    private readonly FinTrackerDbContext _dbContext;
    private readonly HttpClient _client;
    private readonly string _apiKey;

    public HoldingService(FinTrackerDbContext dbContext, IConfiguration configuration, HttpClient client)
    {
        _dbContext = dbContext;

        _client = client;
        // Take the API key from the configuration, if it doesn't exist, throw an exception.
        _apiKey = configuration["Finnhub:ApiKey"]
                  ?? throw new ArgumentNullException("There is no API Key in the configuration!");
    }

    public async Task<IEnumerable<HoldingDTO>> GetHoldingsAsync(int userId)
    {
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

        var dtoTasks = holdingsFromDb.Select(async h => new HoldingDTO
        {
            Id = h.Id,
            TickerSymbol = h.TickerSymbol,
            StockName = h.StockName,
            Quantity = h.Quantity,
            BuyPrice = h.BuyPrice,
            CurrentPrice = (await GetStockData(h.TickerSymbol))?.CurrentPrice ?? 0,
            CurrencyCode = h.CurrencyCode,
            CurrencyPriceWhenBought = h.CurrencyPrice,
            CurrentCurrencyPrice = await GetExchangeRateFrankfurter(h.CurrencyCode.ToString(), "PLN"),
            UserId = userId
        });

        return await Task.WhenAll(dtoTasks);
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
                CurrentPrice = (await GetStockData(holdingEntity.TickerSymbol))?.CurrentPrice ?? 0,
                CurrencyCode = holdingEntity.CurrencyCode,
                CurrentCurrencyPrice = await GetExchangeRateFrankfurter(holdingEntity.CurrencyCode.ToString(), "PLN"),
                CurrencyPriceWhenBought = holdingEntity.CurrencyPrice,
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
                ((currentDataOfThisHolding.BuyPrice * (decimal)currentDataOfThisHolding.Quantity) + 
                (createHoldingDTO.BuyPrice * (decimal)createHoldingDTO.Quantity)) / 
                (decimal)(currentDataOfThisHolding.Quantity + createHoldingDTO.Quantity);
            // Calculate the new average currency price based on the existing quantity and currency price,
            // and the new quantity and currency price.
            currentDataOfThisHolding.CurrencyPrice =
                ((currentDataOfThisHolding.CurrencyPrice * (decimal)currentDataOfThisHolding.Quantity) +
                ((decimal)createHoldingDTO.CurrencyPrice * (decimal)createHoldingDTO.Quantity)) /
                (decimal)(currentDataOfThisHolding.Quantity + createHoldingDTO.Quantity);

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
        string url = $"https://finnhub.io/api/v1/quote?symbol={symbol}&token={_apiKey}";

        var response = await _client.GetFromJsonAsync<StockDataResponse>(url);

        return response;
    }
}
