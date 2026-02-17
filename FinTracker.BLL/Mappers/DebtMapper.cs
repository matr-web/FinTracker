using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.DebtDTOs;
using FinTracker.Models.DTOs.InstallmentDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using System.Linq.Expressions;

namespace FinTracker.BLL.Mappers;

public class DebtMapper
{
    /// <summary>
    /// Static mapping expression from DebtEntity to DebtDTO.
    /// </summary>
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
