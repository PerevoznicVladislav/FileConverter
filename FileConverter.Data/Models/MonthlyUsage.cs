using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data.Models;

[Table("MonthlyUsage")]
[Index("UserId", "Year", "Month", Name = "UQ_MonthlyUsage_User_Year_Month", IsUnique = true)]
public partial class MonthlyUsage
{
    [Key]
    [Column("monthly_usage_id")]
    public Guid MonthlyUsageId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("year")]
    public int Year { get; set; }

    [Column("month")]
    public int Month { get; set; }

    [Column("conversions_used")]
    public int ConversionsUsed { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("MonthlyUsages")]
    public virtual User User { get; set; } = null!;
}
