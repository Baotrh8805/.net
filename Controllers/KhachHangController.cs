using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]
using Project.Data; // Namespace của DbContext
using System.Linq;
using System.Security.Claims; // Required for Claims
using Project.Models; 
using Project.Models.KhachHang;

namespace Project.Controllers
{
    [Authorize(Roles = "Khách")]
    public class KhachHangController : Controller
    {
        private readonly ProjetcNetQuanLyMayTinhContext _context;

        public KhachHangController(ProjetcNetQuanLyMayTinhContext context)
        {
            _context = context;
        }

        // Action Home: Trang chủ dành cho khách hàng
        public IActionResult Home()
        {
            // Lấy thông tin MaNguoiDung từ Claims
            string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(maNguoiDung))
            {
                // Nếu không tìm thấy MaNguoiDung, chuyển hướng về trang đăng nhập
                return RedirectToAction("DangNhap", "Home");
            }

            // Lấy thông tin người dùng từ cơ sở dữ liệu
            var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == maNguoiDung);

            if (nguoiDung == null)
            {
                // Nếu không tìm thấy người dùng, chuyển hướng về trang đăng nhập
                return RedirectToAction("DangNhap", "Home");
            }
            
            var viewModel = new KhachHangHomeViewModel
            {
                HoTen = nguoiDung.HoTen,
                SoDienThoai = nguoiDung.SoDienThoai,
                SoDu = nguoiDung.SoDu ?? 0, // Nếu SoDu null, gán giá trị mặc định là 0
                SuccessMessage = nguoiDung.HoTen
            };

            return View(viewModel);
        }


        // GET: Chọn máy
[HttpGet]
public IActionResult ChonMay()
{
    var danhSachMay = _context.MayTinhs.ToList();
    return View(danhSachMay);
}

// POST: Đặt máy
[HttpPost]
public IActionResult DatMay(string maMay)
{
    string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? string.Empty;

    if (string.IsNullOrEmpty(maNguoiDung))
    {
        TempData["Error"] = "Không xác định được người dùng.";
        return RedirectToAction("DangNhap", "Home");
    }

    var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == maNguoiDung);
    var may = _context.MayTinhs.FirstOrDefault(m => m.MaMay == maMay);
    Console.WriteLine(">>> Trạng thái máy: " + may.TrangThai);
   if (nguoiDung == null || may == null || may.TrangThai?.Trim().ToLower() != "sẵn sàng")
    {
        TempData["Error"] = "Không thể đặt máy. Vui lòng kiểm tra lại thông tin.";
        return RedirectToAction("ChonMay");
    }
   

    may.TrangThai = "Đang sử dụng";

    var suDung = new SuDungMay
    {
        MaSuDung = Guid.NewGuid().ToString().Substring(0, 8),
        MaNguoiDung = nguoiDung.MaNguoiDung,
        MaMay = may.MaMay,
        ThoiGianBatDau = DateTime.Now
    };

    _context.SuDungMays.Add(suDung);
    _context.SaveChanges();

    TempData["Success"] = "Đặt máy thành công!";
    return RedirectToAction("Home");
}

        
    }




}