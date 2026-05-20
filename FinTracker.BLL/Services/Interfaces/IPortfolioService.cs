using FinTracker.Models.DTOs.PortfolioDTOs;
using FinTracker.Models.ViewModels;

namespace FinTracker.BLL.Services.Interfaces;

/// <summary>
/// Provides operations for managing and retrieving user investment portfolios.
/// </summary>
public interface IPortfolioService
{
    /// <summary>
    /// Retrieves the current portfolio data for a specific user, calculating real-time values, exchange rates, and percentage changes.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A <see cref="PortfolioViewModel"/> containing the aggregated current portfolio data and individual holdings.</returns>
    Task<PortfolioViewModel> GetCurrentPortfolioDataAsync(int userId);

    /// <summary>
    /// Retrieves the historical portfolio snapshots for a specific user, ordered by date.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>A collection of <see cref="PortfolioDTO"/> representing the user's portfolio history.</returns>
    Task<IEnumerable<PortfolioDTO>> GetPortfolioHistoryAsync(int userId);

    /// <summary>
    /// Retrieves a specific historical portfolio snapshot by its unique identifier.
    /// </summary>
    /// <param name="portfolioId">The unique identifier of the portfolio snapshot.</param>
    /// <returns>A <see cref="PortfolioDTO"/> if found; otherwise, null.</returns>
    Task<PortfolioDTO?> GetPortfolioSaveByIdAsync(int portfolioId);

    /// <summary>
    /// Saves the current state of the user's portfolio as a historical snapshot for the first day of the current month.
    /// Overwrites any existing snapshot for the current month.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The unique identifier of the newly saved portfolio snapshot.</returns>
    /// <exception cref="ArgumentException">Thrown when portfolio data cannot be retrieved for saving.</exception>
    Task<int> SaveCurrentPortfolioDataAsync(int userId);

    /// <summary>
    /// Saves a manually provided historical portfolio snapshot for a specific past date.
    /// Overwrites any existing snapshot for that specific month.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <param name="savePortfolioDTO">The data transfer object containing the historical portfolio details.</param>
    /// <returns>The unique identifier of the newly saved historical portfolio snapshot.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the provided date is in the future.</exception>
    Task<int> SaveHistoricalPortfolioDataAsync(int userId, SavePortfolioDTO savePortfolioDTO);

    /// <summary>
    /// Deletes a specific historical portfolio snapshot for a given user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user owning the portfolio.</param>
    /// <param name="portfolioId">The unique identifier of the portfolio snapshot to delete.</param>
    /// <returns><c>true</c> if the deletion was successful.</returns>
    /// <exception cref="ArgumentException">Thrown when no portfolio record matches the given identifiers.</exception>
    Task<bool> DeleteSinglePortfolioAsync(int userId, int portfolioId);

    /// <summary>
    /// Deletes all historical portfolio data associated with a specific user.
    /// </summary>
    /// <param name="userId">The unique identifier of the user.</param>
    /// <returns>The number of deleted records directly in the database.</returns>
    Task<int> DeleteWholePortfolioAsync(int userId);
}
