using System;
using System.Collections.Generic;


using System.ComponentModel.DataAnnotations;

namespace Project.Data;

public partial class NapTien
{
    [Key]
    public string MaNapTien { get; set; } = null!;

    public string MaNguoiDung { get; set; } = null!;

    public decimal SoTien { get; set; }

    public DateTime? ThoiGianNap { get; set; }

    public string PhuongThuc { get; set; } = null!;

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
