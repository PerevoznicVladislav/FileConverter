using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data.Models;

[Index("Email", Name = "UQ_Users_Email", IsUnique = true)]
public partial class User
{
    [Key]
    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("email")]
    [StringLength(256)]
    [Unicode(false)]
    public string Email { get; set; } = null!;

    [Column("password_hash")]
    [StringLength(256)]
    [Unicode(false)]
    public string PasswordHash { get; set; } = null!;

    [Column("created_at")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<Conversion> Conversions { get; set; } = new List<Conversion>();

    [InverseProperty("User")]
    public virtual ICollection<MonthlyUsage> MonthlyUsages { get; set; } = new List<MonthlyUsage>();

    [InverseProperty("User")]
    public virtual UserPlan? UserPlan { get; set; }
}
