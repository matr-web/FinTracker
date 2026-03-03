namespace FinTracker.BLL.Utils;

public static class FinanceUtils
{
    /// <summary>
    /// Calculates the percentage change from a specified buying price to a current price.
    /// </summary>
    /// <remarks>If the buying price is zero, the method returns 0 to prevent a division by zero
    /// error.</remarks>
    /// <param name="buyPrice">The original price at which the item was purchased. Must be greater than zero to avoid division by zero.</param>
    /// <param name="currentPrice">The current price of the item used to determine the change relative to the buying price.</param>
    /// <returns>A decimal value representing the percentage change from the buying price to the current price. Returns 0 if the
    /// buying price is zero.</returns>
    public static decimal CalculatePercentageChange(decimal buyPrice, decimal currentPrice)
    {
        if (buyPrice == 0) return 0;
        return ((currentPrice - buyPrice) / buyPrice) * 100;
    }
}
