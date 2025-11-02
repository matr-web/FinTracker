using System.ComponentModel.DataAnnotations;

namespace FinTracker.DAL.Entities;

public class UserEntity
{
    public int Id { get; set; }

    public required string Username { get; set; }

    [DataType(DataType.EmailAddress)]
    public required string Email { get; set; }

    public required string PasswordHash { get; set; }

    public  ICollection<HistoryEntity>? History { get; set; }

    public ICollection<HoldingEntity>? Holdings { get; set; }

    public ICollection<DebtEntity>? Debts { get; set; }
}
