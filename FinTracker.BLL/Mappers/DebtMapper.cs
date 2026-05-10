using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.DebtDTOs;
using FinTracker.Models.DTOs.InstallmentDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Mappers;

public class DebtMapper
{
    /// <summary>
    /// Gets an expression that projects a DebtEntity to a DebtDTO for use in LINQ queries.
    /// </summary>
    /// <remarks>This projection can be used with Entity Framework or other LINQ providers to efficiently
    /// select only the required fields from the data source. The resulting DebtDTO includes related User and Installments information if
    /// available.</remarks>
    public static Expression<Func<DebtEntity, DebtDTO>> Projection =>
    d => new DebtDTO
    {
        Id = d.Id,
        UserId = d.UserId,
        LoanPurpose = d.LoanPurpose,
        Amount = d.Amount,
        InterestRateProcentage = d.InterestRateProcentage,
        NumberOfInstallments = d.NumberOfInstallments,
        InstallmentAmount = d.InstallmentAmount,
        Date = d.Date,

        User = d.User != null ? new UserDTO()
        {
            Id = d.User.Id,
            Username = d.User.Username,
            Email = d.User.Email
        } : null,

        Installments = d.Installments != null ?
                d.Installments.Select(i => new InstallmentDTO
                {
                    Id = i.Id,
                    DebtId = i.DebtId,
                    AmountLeft = i.AmountLeft,
                    NumberOfInstallment = i.NumberOfInstallment,
                    RepaymentDate = i.RepaymentDate
                }).ToList() : new List<InstallmentDTO>()
    };
}
