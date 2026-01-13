using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FileConverter.Data.Models;

public partial class Conversion
{
    [Key]
    [Column("conversion_id")]
    public Guid ConversionId { get; set; }

    [Column("user_id")]
    public Guid UserId { get; set; }

    [Column("from_type")]
    [StringLength(10)]
    [Unicode(false)]
    public string FromType { get; set; } = null!;

    [Column("to_type")]
    [StringLength(10)]
    [Unicode(false)]
    public string ToType { get; set; } = null!;

    [Column("input_file_name")]
    [StringLength(255)]
    [Unicode(false)]
    public string InputFileName { get; set; } = null!;

    [Column("input_path")]
    [StringLength(500)]
    [Unicode(false)]
    public string InputPath { get; set; } = null!;

    [Column("output_path")]
    [StringLength(500)]
    [Unicode(false)]
    public string? OutputPath { get; set; }

    [Column("status")]
    [StringLength(20)]
    [Unicode(false)]
    public string Status { get; set; } = null!;

    [Column("created_at")]
    [Precision(0)]
    public DateTime CreatedAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Conversions")]
    public virtual User User { get; set; } = null!;
}
