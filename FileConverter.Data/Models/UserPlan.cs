using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data.Models;

[Table("UserPlan")]
[Index("UserId", Name = "UQ_UserPlan_User", IsUnique = true)]
public partial class UserPlan
{
    [Key]
    [Column("user_plan_id")]
    public Guid UserPlanId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("plan_id")]
    public Guid PlanId { get; set; }

    [Column("start_date")]
    [Precision(0)]
    public DateTime StartDate { get; set; }

    [ForeignKey("PlanId")]
    [InverseProperty("UserPlans")]
    public virtual Plan Plan { get; set; } = null!;

    [ForeignKey("UserId")]
    [InverseProperty("UserPlan")]
    public virtual User User { get; set; } = null!;
}
