using System.ComponentModel.DataAnnotations;

namespace Project.Models.KhachHang
{
    public class ChonMayViewModel
    {
        public string? MaMay { get; set; } // Mã máy

        [Required(ErrorMessage = "Tên máy là bắt buộc.")]
        public string? TenMay { get; set; } // Tên máy

        [Required(ErrorMessage = "Trạng thái là bắt buộc.")]
        public string? TrangThai { get; set; } // Trạng thái (Sẵn sàng, Đang sử dụng, Bảo trì)

        [Required(ErrorMessage = "Đơn giá là bắt buộc.")]
        [Range(0, double.MaxValue, ErrorMessage = "Đơn giá phải là số dương.")]
        public double DonGia { get; set; } // Đơn giá sử dụng máy

        public string? MoTa { get; set; } // Mô tả chi tiết về máy

        public bool IsSelected { get; set; } // Trạng thái chọn máy (true nếu được chọn)
    }
}