using System;
using System.Collections.Generic;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Project.Data;


[Table("MayTinh")]
public partial class MayTinh
{
    [Key] // ✅ Thêm dòng này để đánh dấu khóa chính
    [Column("MaMay")]
    [StringLength(10)]
    public string MaMay { get; set; } = null!;

    [StringLength(50)]
    public string TenMay { get; set; } = null!;

    [StringLength(30)]
    public string? TrangThai { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal DonGia { get; set; }

    public string? MoTa { get; set; }

    public DateTime? ThoiGianTao { get; set; }

    public DateTime? ThoiGianXoa { get; set; }

    public virtual ICollection<SuDungMay> SuDungMays { get; set; } = new List<SuDungMay>();
}

