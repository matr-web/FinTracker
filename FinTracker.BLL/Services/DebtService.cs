using FinTracker.BLL.Mappers;
using FinTracker.BLL.Services.Interfaces;
using FinTracker.BLL.Utils;
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

    /// <inheritdoc cref="IDebtService.GetSummedDebtAsync" />
    public async Task<IEnumerable<SummedDebtDTO>?> GetSummedDebtAsync(int userId, int? monthsCount)
    {
        // 1. Get all debts of the user with their installments, but only the necessary data
        // (Id, Amount, Date for debts and AmountLeft, RepaymentDate for installments).
        var debts = await _dbContext.Debts
            .Where(d => d.UserId == userId)
            .Select(d => new {
                d.Id,
                d.Amount,
                d.Date,
                Installments = d.Installments != null
                           ? d.Installments.Select(i => new { i.AmountLeft, i.RepaymentDate })
                           : Enumerable.Empty<dynamic>()
            })
            .ToListAsync();

        if (!debts.Any())
            return Enumerable.Empty<SummedDebtDTO>();

        // 2. Flatten all "debt change" events
        var events = debts.SelectMany(d =>
        {
            // For each debt, we create a list of "events" that represent changes in the debt amount over time.
            var list = new List<BasicDebtData> { new() { Id = d.Id, AmountLeft = d.Amount, Date = d.Date } };
            // Then we add an event for each installment that was paid off, which reduces the amount left.
            list.AddRange(d.Installments.Select(i => new BasicDebtData { Id = d.Id, AmountLeft = i.AmountLeft, Date = i.RepaymentDate }));
            return list;
        })
        .OrderBy(e => e.Date) // Order all events by date, so we can process them in chronological order.
        .ToList();

        var result = new List<SummedDebtDTO>();
        var currentDebtStates = new Dictionary<int, decimal>(); // DebtId -> Last known amount

        var firstDate = events.First().Date;
        var lastDate = events.Last().Date;

        // 3. Iterate month by month from the start date to the end date
        var currentDate = firstDate.ToFirstDayOfMonth();
        var endDate = lastDate.ToLastDayOfMonth();

        while (currentDate <= endDate)
        {
            // Find all changes that happened in THIS month
            var monthlyEvents = events
                .Where(e => e.Date.Year == currentDate.Year && e.Date.Month == currentDate.Month)
                .ToList();

            // Update the current state of each debt based on the events of this month
            foreach (var e in monthlyEvents)
            {
                // Overwrite the state for the given debt (we are interested in the last value of the month)
                currentDebtStates[e.Id] = e.AmountLeft;
            }

            // Sum the current values of all known debts
            result.Add(new SummedDebtDTO
            {
                Date = currentDate,
                AmountLeft = currentDebtStates.Values.Sum()
            });

            // Move to the next month
            currentDate = currentDate.AddMonths(1);
        }

        // 4. If the user specified a period of time, take only the last "monthsCount" months from the result. Else return everything.
        return monthsCount == null ? result : result.TakeLast(monthsCount.Value);
    }

    /// <inheritdoc cref="IDebtService.GetAllDebtsAsync" />
    public async Task<IEnumerable<DebtDTO>> GetAllDebtsAsync(int userId)
    {
        return await _dbContext.Debts
            .Where(d => d.UserId == userId)
            .Select(DebtMapper.Projection)
            .ToListAsync();
    }

    /// <inheritdoc cref="IDebtService.GetSingleDebtAsync" />
    public async Task<DebtDTO?> GetSingleDebtAsync(int userId, int debtId)
    {
        return await _dbContext.Debts
            .Where(d => d.Id == debtId && d.UserId == userId)
            .Select(DebtMapper.Projection)
            .FirstOrDefaultAsync();
    }

    /// <inheritdoc cref="IDebtService.InsertDebtAsync" />
    public async Task<int> InsertDebtAsync(CreateDebtDTO createDebtDTO, int userId)
    {
        var debtEntity = new DebtEntity
        {
            LoanPurpose = createDebtDTO.LoanPurpose,
            Amount = createDebtDTO.Amount,
            InterestRateProcentage = createDebtDTO.InterestRateProcentage,
            NumberOfInstallments = createDebtDTO.NumberOfInstallments,
            InstallmentAmount = createDebtDTO.InstallmentAmount,
            // If the user provided a date, take it and change it to the first day of the month, because the charts are on a monthly scale
            // e.g., if the user provides 15th March, it will be changed to 1st March. Else take the first day of the current month and year.
            Date = createDebtDTO?.Date?.ToFirstDayOfMonth()
                 ?? DateOnly.FromDateTime(DateTime.Today).ToFirstDayOfMonth(),
            UserId = userId
        };

        await _dbContext.Debts.AddAsync(debtEntity);
        await _dbContext.SaveChangesAsync();

        return debtEntity.Id;
    }

    /// <inheritdoc cref="IDebtService.PayOffInstallmentAsync" />
    public async Task<DebtDTO?> PayOffInstallmentAsync(int userId, int debtId, RepayInstallmentDTO repayInstallmentDTO)
    {
        // 1. Find the debt that needs to be repaid. 
        var debt = await _dbContext.Debts
            .Include(d => d.Installments)
            .FirstOrDefaultAsync(d => d.Id == debtId && d.UserId == userId && !d.IsPaidOff);

        // 2. If there is no such debt, throw an exception.
        if (debt == null)
            throw new KeyNotFoundException($"Debt with ID {debtId} not found.");

        // 3. Check if it is not already paid off. If it is, throw an exception. 
        if (debt.IsPaidOff)
            throw new InvalidOperationException("Debt is already paid off.");

        // 4. Get the date of today and change it to the first day of the month, because the charts are on a monthly scale.
        var firstOfCurrentMonth = DateTime.Today.ToFirstDayOfMonth();

        // 5. Find last installment to the debt, to check how many installments have already been paid off and how much is left.
        var lastInstallment = debt.Installments?
        .OrderByDescending(i => i.NumberOfInstallment)
        .FirstOrDefault();

        // 6. If the user has provided a repayment date validate it.
        if (repayInstallmentDTO.RepaymentDate != null)
        {
            ValidateRepaymentDate(debt, repayInstallmentDTO, lastInstallment);
        }

        // 7. Create new installment entity that will be added to the db - it will represent the just paid installment
        var newInstallment = new InstallmentEntity
        {
            DebtId = debtId,
            // If the user provided a repayment date, take the month and year from it. Else just take the first day from current month and year
            RepaymentDate = repayInstallmentDTO.RepaymentDate != null ?
            DateOnly.FromDateTime(new DateTime(repayInstallmentDTO.RepaymentDate.Value.Year, repayInstallmentDTO.RepaymentDate.Value.Month, 1))
            : DateOnly.FromDateTime(firstOfCurrentMonth),
            NumberOfInstallment = (lastInstallment?.NumberOfInstallment ?? 0) + 1,
            // If there is no last installment, it means that this is the first installment that is being paid off,
            // so the AmountLeft is the general debt amount minus the installment amount.
            // Else, it is the AmountLeft from last installment minus the installment amount.
            // In both cases, if the result is negative, set it to 0, because the user cannot pay more than he owes.
            AmountLeft = Math.Max(0, (lastInstallment?.AmountLeft ?? debt.Amount) - debt.InstallmentAmount)
        };

        // 8. If this is the last installment set the debt status as paid off
        if (newInstallment.NumberOfInstallment >= debt.NumberOfInstallments || newInstallment.AmountLeft <= 0)
        {
            debt.IsPaidOff = true;
        }

        // 9. Add the new installment to the database and save changes
        await _dbContext.Installments.AddAsync(newInstallment);
        await _dbContext.SaveChangesAsync();

        // 10. Return the updated debt with the new installment
        return await GetSingleDebtAsync(userId, debtId);
    }

    /// <inheritdoc cref="IDebtService.DeleteSingleDebtAsync" />
    public async Task<bool> DeleteSingleDebtAsync(int userId, int debtId)
    {
        // ExecuteDeleteAsync sends "DELETE FROM Debt WHERE Id = @id AND UserId = @userId" directly to the database
        int deletedRows = await _dbContext.Debts
            .Where(d => d.Id == debtId && d.UserId == userId)
            .ExecuteDeleteAsync();

        if (deletedRows == 0)
        {
            throw new ArgumentException($"There was no debt record with given id: {debtId}");
        }

        return true;
    }

    /// <inheritdoc cref="IDebtService.DeleteWholeDebtAsync" />
    public async Task<int> DeleteWholeDebtAsync(int userId)
    {
        // ExecuteDeleteAsync sends "DELETE FROM Debt WHERE UserId = @userId" directly to the database
        int deletedRows = await _dbContext.Debts
            .Where(d => d.UserId == userId)
            .ExecuteDeleteAsync();

        return deletedRows;
    }

    private void ValidateRepaymentDate(DebtEntity debt, RepayInstallmentDTO repayInstallmentDTO, InstallmentEntity? lastInstallment)
    {
        // Check if this date is not earlier than the date the user incurred the debt.
        if (repayInstallmentDTO.RepaymentDate < debt.Date.ToFirstDayOfMonth())
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(repayInstallmentDTO.RepaymentDate),
                actualValue: repayInstallmentDTO.RepaymentDate,
                $"The given repayment date: {repayInstallmentDTO.RepaymentDate} is earlier than the debt date: {debt.Date}."
                );
        }

        // Check if this date is not earlier than the last installment date, if there are some installments already paid off.
        if (lastInstallment != null && lastInstallment.RepaymentDate > repayInstallmentDTO.RepaymentDate)
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(repayInstallmentDTO.RepaymentDate),
                actualValue: repayInstallmentDTO.RepaymentDate,
                $"The given repayment date: {repayInstallmentDTO.RepaymentDate} is earlier than the last installment date: " +
                $"{lastInstallment.RepaymentDate}."
                );
        }

        // Get the last day of the current month, to check if the user provided date is not from the future.
        var lastOfCurrentMonth = DateOnly.FromDateTime(DateTime.Today.ToLastDayOfMonth());

        // Check if the user provided date is not from the future months.
        if (repayInstallmentDTO.RepaymentDate > lastOfCurrentMonth)
        {
            throw new ArgumentOutOfRangeException(
                paramName: nameof(repayInstallmentDTO.RepaymentDate),
                actualValue: repayInstallmentDTO.RepaymentDate,
                $"The given repayment date: {repayInstallmentDTO.RepaymentDate} is from a future month." +
                $" The latest acceptable date: {lastOfCurrentMonth}."
                );
        }
    }
}
