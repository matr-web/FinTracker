using FinTracker.BLL.Mappers;
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
        // Find all debts of a given user and order them by the date.
        var debts = _dbContext.Debts
            .Include(d => d.Installments)
            .Where(d => d.UserId == userId);

        // Take all the data from user debts and the installments he already paid off.
        // Holds Debt Id and its value at given date. Holds only those dates in which something changed:
        // The Debt was created or an installment has been paid off.
        var debtData = new List<BasicDebtData>();

        foreach (var debt in debts)
        {
            // At first take data if from the general debt data.
            // So you get the general debt amount and date of the debt.
            debtData.Add(new BasicDebtData()
            {
                Id = debt.Id,
                AmountLeft = debt.Amount,
                Date = debt.Date
            });

            // If there are some installments to the debt
            // take the data from all the installments.
            if (debt.Installments != null && debt.Installments.Any())
            {
                foreach (var installment in debt.Installments)
                {
                    debtData.Add(new BasicDebtData()
                    {
                        Id = debt.Id,
                        AmountLeft = installment.AmountLeft,
                        Date = installment.RepaymentDate
                    });
                }
            }
        }

        // Order the debts by date.
        var orderDebtData = debtData.OrderBy(d => d.Date).ToList();

        var result = new List<SummedDebtDTO>();
        decimal runningTotal = 0;

        for (int i = 0; i < debtData.Count(); i++)
        {
            // if we are working on a given debt for the first time, simply sum the AmountLeft to the runningTotal variable.
            // which stores the total value of the debt.
            if (orderDebtData.Take(i).ToList().Count(data => data.Id == orderDebtData[i].Id) == 0)
            {
                runningTotal += orderDebtData[i].AmountLeft;
            }
            else // If we already have worked on a given debt.
            {
                // Find the data of the debt from the the nearest date.
                var lastData = orderDebtData.Take(i).Last(d => d.Id == orderDebtData[i].Id);

                // Remove the amount of the debt from runningTotal given last time.
                runningTotal = runningTotal - lastData.AmountLeft;

                // Add the amount from current date.
                runningTotal += orderDebtData[i].AmountLeft;
            }

            // Add it to the result collection.
            result.Add(new SummedDebtDTO
            {
                Date = orderDebtData[i].Date,
                AmountLeft = runningTotal
            });
        }

        // Group the result by the date, and return the latest Amount as it is the most up-to-date.
        return result.GroupBy(r => r.Date).Select(g => new SummedDebtDTO()
        {
            Date = g.Key,
            AmountLeft = g.Last().AmountLeft
        });
    }

    public IQueryable<DebtDTO?> GetAllDebts(int userId)
    {
        return _dbContext.Debts
            .Include(d => d.User)
            .Include(d => d.Installments)
            .Where(d => d.UserId == userId)
            .Select(DebtMapper.Projection);
    }

    public async Task<DebtDTO?> GetSingleDebtAsync(int debtId)
    {
        return await _dbContext.Debts
            .Include(d => d.User)
            .Include(d => d.Installments)
            .Where(d => d.Id == debtId)
            .Select(DebtMapper.Projection)
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

            // If the date was given take the month and year from it. 
            // Else just take the first day from current month and year.
            Date = createDebtDTO.Date != null ?
            new DateOnly(createDebtDTO.Date.Value.Year, createDebtDTO.Date.Value.Month, 1) :
            DateOnly.FromDateTime(new DateTime(today.Year, today.Month, 1)),

            UserId = userId
        };

        await _dbContext.Debts.AddAsync(debtEntity);
        await _dbContext.SaveChangesAsync();

        return debtEntity.Id;
    }

    public async Task<DebtDTO?> PayOffInstallmentAsync(int debtId, RepayInstallmentDTO repayInstallmentDTO)
    {
        // Find the debt that needs to be repaid. If there is no such debt throw: InvalidOperationException.
        var debt = await _dbContext.Debts
            .Include(d => d.Installments)
            .SingleAsync(d => d.Id == debtId);

        // Check if such debt exists. If not throw: InvalidOperationException.
        if (debt == null)
        {
            throw new InvalidOperationException($"There is no debt with ID {debtId}.");
        }

        // Check if the debt is already paid off.
        if (debt.IsPaidOff)
        {
            throw new InvalidOperationException($"Debt with ID {debt.Id} is already paid off.");
        }

        // Saves the today date. It is used in multiple places in this method, so it is better to save it as a variable. DRY.
        var today = DateOnly.FromDateTime(new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1));

        // If the user has provided a repayment date.
        if (repayInstallmentDTO.RepaymentDate != null)
        {
            // Check if this date is not earlier than the date the user incurred the debt.
            if (repayInstallmentDTO.RepaymentDate < DateOnly.FromDateTime(new DateTime(debt.Date.Year, debt.Date.Month, 1)))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(repayInstallmentDTO.RepaymentDate),
                    actualValue: repayInstallmentDTO.RepaymentDate,
                    $"The given repayment date: {repayInstallmentDTO.RepaymentDate} is earlier than the debt date: {debt.Date}."
                    );
            }

            // Check if this date is not earlier than the last intallment date.
            if (debt.Installments != null && debt.Installments.Count() > 0 
                && debt.Installments.OrderBy(i => i.RepaymentDate).Last().RepaymentDate > repayInstallmentDTO.RepaymentDate)
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(repayInstallmentDTO.RepaymentDate),
                    actualValue: repayInstallmentDTO.RepaymentDate,
                    $"The given repayment date: {repayInstallmentDTO.RepaymentDate} is earlier than the last installment date: " +
                    $"{debt.Installments.OrderBy(i => i.RepaymentDate).Last().RepaymentDate}."
                    );
            }

            // If this date is from the future.
            if (repayInstallmentDTO.RepaymentDate > DateOnly.FromDateTime(new DateTime(today.Year, today.Month, DateTime.Today.Day)))
            {
                throw new ArgumentOutOfRangeException(
                    paramName: nameof(repayInstallmentDTO.RepaymentDate),
                    actualValue: repayInstallmentDTO.RepaymentDate,
                    $"The given repayment date: {repayInstallmentDTO.RepaymentDate} is from the future. Today date: " +
                    $"{today}."
                    );
            }
        }

        // Find if there are already some installments to the debt. If so assign it to the _installment variable.
        var _installment = debt.Installments != null && debt.Installments.Count() > 0 ?
            debt.Installments.OrderBy(i => i.NumberOfInstallment).LastOrDefault() : null;

        // Create new installment entity that will be added to the db - it will represent the just paid installment.
        var installment = new InstallmentEntity
        {
            DebtId = debtId,

            // Set the date to the one given by the user (but change the day to first of the month)
            // or to the first day of a given month, because the charts are on a monthly scale.
            RepaymentDate = repayInstallmentDTO.RepaymentDate != null ?
            DateOnly.FromDateTime(new DateTime(repayInstallmentDTO.RepaymentDate.Value.Year, repayInstallmentDTO.RepaymentDate.Value.Month, 1))
            : today
        };

        // If there is no installment to the dept.
        if (_installment == null)
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
            installment.AmountLeft = _installment.AmountLeft - debt.InstallmentAmount;

            // Add 1 to the Installment number.
            installment.NumberOfInstallment = _installment.NumberOfInstallment + 1;
        }

        // If this is the last installment set the debt status as paid off.
        if (installment.NumberOfInstallment >= debt.NumberOfInstallments) 
        {
            debt.IsPaidOff = true;
            _dbContext.Debts.Update(debt);
        }

        await _dbContext.Installments.AddAsync(installment);
        await _dbContext.SaveChangesAsync();

        return await _dbContext.Debts
            .Include(d => d.User)
            .Include(d => d.Installments)
            .Where(d => d.Id == debtId)
            .Select(DebtMapper.Projection)
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
