using FinTracker.BLL.Services.Interfaces;
using FinTracker.DAL.EF;
using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.DebtDTOs;
using FinTracker.Models.DTOs.InstallmentDTOs;
using Microsoft.EntityFrameworkCore;

namespace FinTracker.BLL.Services;

public class DebtService : IDebtService
{
    private readonly FinTrackerDbContext _dbContext;

    public DebtService(FinTrackerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public IEnumerable<SummedDebtDTO?> GetSummedDebt(int userId)
    {
        // Find all debts of a given user.
        var debts = _dbContext.Debts
            .Include(d => d.Installments)
            .Where(d => d.UserId == userId);

        // Take all the data from user debts and the installments he already paid off.
        var allInstallments = new List<SummedDebtDTO>();

        foreach (var debt in debts)
        {
            // If there are some installments of the debt
            // take the data from all the installments.
            if (debt.Installments != null && debt.Installments.Any())
            {
                foreach (var installment in debt.Installments)
                {
                    allInstallments.Add(new SummedDebtDTO()
                    {
                        AmountLeft = installment.AmountLeft,
                        RepaymentDate = installment.RepaymentDate
                    });
                }
            }
            else // Else take if from the general debt data.
            {
                allInstallments.Add(new SummedDebtDTO()
                {
                    AmountLeft = debt.Amount,
                    RepaymentDate = debt.Date
                });
            }
        }

        // Group the data by date,
        // and sum all the debt value for a user for given date.
        // The charts are monthly so we assume that all the dates start with 1, and then contain diff month and year.
        var result = allInstallments
            .GroupBy(i => i.RepaymentDate).Select(g => new SummedDebtDTO()
            { 
                RepaymentDate = g.Key,
                AmountLeft = g.Sum(i => i.AmountLeft)
            });

        return result;
    }

    public IQueryable<DebtDTO?> GetAllDebts(int userId)
    {
        return _dbContext.Debts.Where(d => d.UserId == userId)
            .Select(DebtDTO.Projection);
    }

    public async Task<DebtDTO?> GetSingleDebtAsync(int debtId)
    {
        return await _dbContext.Debts.Where(d => d.Id == debtId)
            .Select(DebtDTO.Projection)
            .FirstOrDefaultAsync();
    }

    public async Task<int> InsertDebtAsync(CreateDebtDTO createDebtDTO, int userId)
    {
        DateTime today = DateTime.Today;

        var debtEntity = new DebtEntity
        {
            LoanPurpose = createDebtDTO.LoanPurpose,
            Amount = createDebtDTO.Amount,
            InterestRateProcentage = createDebtDTO.InterestRateProcentage,
            NumberOfInstallments = createDebtDTO.NumberOfInstallments,
            InstallmentAmount = createDebtDTO.InstallmentAmount,
            Date = createDebtDTO.Date ?? DateOnly.FromDateTime(new DateTime(today.Year, today.Month, 1)),
            UserId = userId
        };

        await _dbContext.Debts.AddAsync(debtEntity);
        await _dbContext.SaveChangesAsync();

        return debtEntity.Id;
    }

    public async Task<DebtDTO?> PayOffInstallment(RepayInstallmentDTO repayInstallmentDTO)
    {
        // Find the debt that needs to be repaid.
        var debt = await _dbContext.Debts.FirstOrDefaultAsync(d => d.Id == repayInstallmentDTO.DebtId);

        if (debt == null)
        {
            throw new ArgumentException("Debt with given Id not found.");
        }

        // Create new installment entity that will be added to the db - it will represent the just paid installment.
        DateTime today = DateTime.Today;

        var installment = new InstallmentEntity
        {
            DebtId = repayInstallmentDTO.DebtId,
            // Set the date to the one given by the user (but change the day to first of the month)
            // or to the first day of a given month, because the charts are on a monthly scale.
            RepaymentDate = repayInstallmentDTO.RepaymentDate != null ?
            DateOnly.FromDateTime(new DateTime(repayInstallmentDTO.RepaymentDate.Value.Year, repayInstallmentDTO.RepaymentDate.Value.Month, 1))
            : DateOnly.FromDateTime(new DateTime(today.Year, today.Month, 1))
        };

        // Find if there are already some installments to the debt.
        var _installment = await _dbContext.Installments.AnyAsync(i => i.DebtId == repayInstallmentDTO.DebtId);

        // If there is no installment to the dept.
        if (_installment == false)
        {
            // Calculate the amount left by: the debt amount - installment amount.
            // Because its the first installment paid off.
            installment.AmountLeft = debt.Amount - debt.InstallmentAmount;

            // First installment...
            installment.NumberOfInstallment = 1;
        }
        else
        {
            // If there is a installment calculate the amount by: amount left - installment amount.
            installment.AmountLeft -= debt.InstallmentAmount;

            // Add 1 to the Installment number.
            installment.NumberOfInstallment += 1;
        } 

        await _dbContext.Installments.AddAsync(installment);
        await _dbContext.SaveChangesAsync();

        return await _dbContext.Debts.Where(d => d.Id == repayInstallmentDTO.DebtId)
            .Select(DebtDTO.Projection)
            .FirstOrDefaultAsync();
    }

    public async Task DeleteSingleDebtAsync(int debtId)
    {
        var obj = _dbContext.Debts.SingleOrDefault(d => d.Id == debtId);

        if (obj != null)
        {
            _dbContext.Debts.Remove(obj);
            await _dbContext.SaveChangesAsync();
        }
    }

    public async Task DeleteWholeDebtAsync(int userId)
    {
        var objects = _dbContext.Debts.Where(d => d.UserId == userId).ToList();

        if (objects.Count > 0)
        {
            _dbContext.Debts.RemoveRange(objects);
            await _dbContext.SaveChangesAsync();
        }
    }
}
