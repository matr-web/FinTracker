using FinTracker.Models.DTOs.PortfolioDTOs;
using FinTracker.Models.ViewModels;

namespace FinTracker.BLL.Services.Interfaces;

public interface IPortfolioService
{
    Task<PortfolioViewModel> GetPortfolioAsync(int userId);

    IQueryable<PortfolioDTO> GetPortfolioHistory(int userId);

    Task<PortfolioDTO?> GetPortfolioSaveByIdAsync(int portfolioId);

    Task<int> SaveCurrentPortfolioDataAsync(int userId);

    Task<int> SaveHistoricalPortfolioDataAsync(int userId, SavePortfolioDTO savePortfolioDTO);
}
