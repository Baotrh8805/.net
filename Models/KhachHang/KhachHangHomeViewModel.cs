using System.ComponentModel.DataAnnotations;

namespace Project.Models.KhachHang
{
    public class KhachHangHomeViewModel
    {
        public string? HoTen { get; set; }
        public string? SoDienThoai { get; set; }
        public decimal SoDu { get; set; }
        public string? SuccessMessage { get; set; }
    
         // Thông tin sử dụng máy
        public bool DangSuDungMay { get; set; } // Đang sử dụng máy hay không
        public DateTime? ThoiGianBatDau { get; set; } // Thời gian bắt đầu sử dụng máy
        public double ThoiGianSuDung { get; set; } // Thời gian sử dụng (giờ)
        public double SoTienConLai { get; set; } // Số tiền còn lại
            public decimal ChiPhi { get; set; } // Thêm thuộc tính Chi phí

    }
}