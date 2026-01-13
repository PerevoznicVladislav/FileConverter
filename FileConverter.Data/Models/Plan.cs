using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data.Models;

[Index("Name", Name = "UQ_Plans_Name", IsUnique = true)]
public partial class Plan
{
    [Key]
    [Column("plan_id")]
    public Guid PlanId { get; set; }

    [Column("name")]
    [StringLength(50)]
    [Unicode(false)]
    public string Name { get; set; } = null!;

    [Column("monthly_price", TypeName = "decimal(10, 2)")]
    public decimal MonthlyPrice { get; set; }

    [Column("max_conversion_per_month")]
    public int MaxConversionPerMonth { get; set; }

    [Column("max_upload_bytes")]
    public long MaxUploadBytes { get; set; }

    [Column("created_at")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [Column("description")]
    [StringLength(255)]
    [Unicode(false)]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }

    [InverseProperty("Plan")]
    public virtual ICollection<UserPlan> UserPlans { get; set; } = new List<UserPlan>();
}
