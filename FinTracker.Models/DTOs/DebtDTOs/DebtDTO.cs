using FinTracker.DAL.Entities;
using FinTracker.Models.DTOs.InstallmentDTOs;
using FinTracker.Models.DTOs.UserDTOs;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace FinTracker.Models.DTOs.DebtDTOs;

public class DebtDTO
{
    public int Id { get; set; }

    public int UserId { get; set; }
    public UserDTO? User { get; set; }

    public string? LoanPurpose { get; set; }
    [DataType(DataType.Currency)]
    public decimal Amount { get; set; }
    /// <summary>
    /// Decimal fraction for the interest rate.
    /// e.g. 0.10 is 10%.
    /// </summary>
    public decimal? InterestRateProcentage { get; set; }
    public int NumberOfInstallments { get; set; }
    public decimal InstallmentAmount { get; set; }
    public DateOnly Date { get; set; }
    public bool IsPaidOff { get; set; }

    public ICollection<InstallmentDTO>? Installments { get; set; }

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
