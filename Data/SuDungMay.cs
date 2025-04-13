﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Project.Data;

public partial class SuDungMay
{
    [Key] // <-- thêm dòng này
    public string MaSuDung { get; set; } = null!;

    public string MaMay { get; set; } = null!;

    public string MaNguoiDung { get; set; } = null!;

    public DateTime ThoiGianBatDau { get; set; }

    public DateTime? ThoiGianKetThuc { get; set; }

    public decimal? TongThoiGian { get; set; }

    public decimal? TongTien { get; set; }

    public virtual MayTinh MaMayNavigation { get; set; } = null!;

    public virtual NguoiDung MaNguoiDungNavigation { get; set; } = null!;
}
