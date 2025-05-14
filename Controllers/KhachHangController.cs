using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization; // Required for [Authorize]
using Project.Data; // Namespace của DbContext
using System.Linq;
using System.Security.Claims; // Required for Claims
using Project.Models; 
using Project.Models.KhachHang;
using Microsoft.EntityFrameworkCore; // Required for Include
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
        // public IActionResult Home()
        // {
        //     // Lấy thông tin MaNguoiDung từ Claims
        //     string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? string.Empty;

        //     if (string.IsNullOrEmpty(maNguoiDung))
        //     {
        //         return RedirectToAction("DangNhap", "Home");
        //     }

        //     // Lấy thông tin người dùng từ cơ sở dữ liệu
        //     var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == maNguoiDung);

        //     if (nguoiDung == null)
        //     {
        //         return RedirectToAction("DangNhap", "Home");
        //     }

        //     // Kiểm tra xem người dùng có đang sử dụng máy không
        //     var suDungMay = _context.SuDungMays
        //         .Include(sdm => sdm.MaMayNavigation)
        //         .FirstOrDefault(sdm => sdm.MaNguoiDung == maNguoiDung && sdm.ThoiGianKetThuc == null);

        //     double thoiGianSuDung = 0;

        //     if (suDungMay != null)
        //     {
        //         thoiGianSuDung = (DateTime.Now - suDungMay.ThoiGianBatDau).TotalMinutes;
        //     }

        //     var viewModel = new KhachHangHomeViewModel
        //     {
        //         HoTen = nguoiDung.HoTen,
        //         SoDienThoai = nguoiDung.SoDienThoai,
        //         SoDu = nguoiDung.SoDu ?? 0,
        //         DangSuDungMay = suDungMay != null,
        //         ThoiGianBatDau = suDungMay?.ThoiGianBatDau,
        //         ThoiGianSuDung = thoiGianSuDung
        //     };

        //     return View(viewModel);
        // }
        public IActionResult Home()
{
    string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? string.Empty;

    if (string.IsNullOrEmpty(maNguoiDung))
    {
        return RedirectToAction("DangNhap", "Home");
    }

    var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == maNguoiDung);

    if (nguoiDung == null)
    {
        return RedirectToAction("DangNhap", "Home");
    }

    var suDungMay = _context.SuDungMays
        .Include(sdm => sdm.MaMayNavigation)
        .FirstOrDefault(sdm => sdm.MaNguoiDung == maNguoiDung && sdm.ThoiGianKetThuc == null);

    double thoiGianSuDung = 0;
    decimal chiPhi = 0;
    decimal soDuHienTai = nguoiDung.SoDu ?? 0;

    if (suDungMay != null)
    {
        thoiGianSuDung = (DateTime.Now - suDungMay.ThoiGianBatDau).TotalMinutes;
        var donGia = suDungMay.MaMayNavigation?.DonGia ?? 0;
        chiPhi = (decimal)(thoiGianSuDung / 60) * donGia;

        // Tính số dư tạm thời sau khi trừ chi phí
        soDuHienTai -= chiPhi;
        if (soDuHienTai < 0)
        {
            soDuHienTai = 0; // Đảm bảo số dư không âm
        }
    }

    var viewModel = new KhachHangHomeViewModel
    {
        HoTen = nguoiDung.HoTen,
        SoDienThoai = nguoiDung.SoDienThoai,
        SoDu = soDuHienTai, // Số dư tạm thời sau khi trừ chi phí
        DangSuDungMay = suDungMay != null,
        ThoiGianBatDau = suDungMay?.ThoiGianBatDau,
        ThoiGianSuDung = thoiGianSuDung,
        ChiPhi = chiPhi
    };

    return View(viewModel);
}

        // GET: Chọn máy
        [HttpGet]
        public IActionResult ChonMay()
        {
            var danhSachMay = _context.MayTinhs.ToList();
            if (danhSachMay == null || danhSachMay.Count == 0)
            {
                TempData["Error"] = "Không có máy nào để đặt.";
                return RedirectToAction("Home");
            }
            // Lọc danh sách máy để chỉ lấy những máy có trạng thái "Sẵn sàng"
            var danhSachMaySangSuDung = danhSachMay.Where(m => m.TrangThai== "Sẵn sàng").ToList();

            if (danhSachMaySangSuDung.Count == 0)
            {
                Console.WriteLine(">>> vao day ");
                TempData["Error"] = "Không có máy nào sẵn sàng để đặt.";
                return RedirectToAction("Home");
            }
            return View(danhSachMaySangSuDung);
        }
   
 
        // POST: Đặt máy
        [HttpPost]
        public IActionResult DatMay(string maMay)
        {
            string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? string.Empty;

            Console.WriteLine(">>> MaNguoiDung: " + maNguoiDung);
            Console.WriteLine(">>> maMay: " + maMay);

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

        
    
    
        // GET: Hủy đặt máy

        // [HttpPost]
        // public IActionResult KetThucSuDung()
        // {
        //     string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? string.Empty;

        //     if (string.IsNullOrEmpty(maNguoiDung))
        //     {
        //         TempData["Error"] = "Không xác định được người dùng.";
        //         return RedirectToAction("DangNhap", "Home");
        //     }

        //     var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == maNguoiDung);
        //     if (nguoiDung == null)
        //     {
        //         TempData["Error"] = "Không tìm thấy thông tin người dùng.";
        //         return RedirectToAction("Home");
        //     }

        //     var suDungMay = _context.SuDungMays
        //         .Include(sdm => sdm.MaMayNavigation)
        //         .FirstOrDefault(sdm => sdm.MaNguoiDung == maNguoiDung && sdm.ThoiGianKetThuc == null);

        //     if (suDungMay == null)
        //     {
        //         TempData["Error"] = "Không tìm thấy thông tin sử dụng máy.";
        //         return RedirectToAction("Home");
        //     }

        //     var thoiGianSuDung = (DateTime.Now - suDungMay.ThoiGianBatDau).TotalMinutes;
        //     var donGia = suDungMay.MaMayNavigation?.DonGia ?? 0;
        //     var chiPhi = (decimal)(thoiGianSuDung / 60) * donGia;

        //     // Kiểm tra nếu số dư nhỏ hơn chi phí
        //     if ((nguoiDung.SoDu ?? 0) < chiPhi)
        //     {
        //         // Tự động kết thúc sử dụng
        //         suDungMay.ThoiGianKetThuc = DateTime.Now;
        //         suDungMay.TongTien = chiPhi;
        //         suDungMay.TongThoiGian = (decimal)thoiGianSuDung;

        //         var may = suDungMay.MaMayNavigation;
        //         if (may != null)
        //         {
        //             may.TrangThai = "Sẵn sàng";
        //             _context.MayTinhs.Update(may);
        //         }

        //         _context.SuDungMays.Update(suDungMay);
        //         _context.SaveChanges();

        //         TempData["Error"] = "Số dư không đủ. Hệ thống đã tự động kết thúc sử dụng.";
        //         return RedirectToAction("Home");
        //     }

        //     // Trừ số dư và kết thúc sử dụng
        //     nguoiDung.SoDu = (nguoiDung.SoDu ?? 0) - chiPhi;
        //     suDungMay.ThoiGianKetThuc = DateTime.Now;

        //     var mayKetThuc = suDungMay.MaMayNavigation;
        //     if (mayKetThuc != null)
        //     {
        //         mayKetThuc.TrangThai = "Sẵn sàng";
        //         _context.MayTinhs.Update(mayKetThuc);
        //     }

        //     _context.NguoiDungs.Update(nguoiDung);
        //     _context.SuDungMays.Update(suDungMay);
        //     _context.SaveChanges();

        //     TempData["Success"] = "Kết thúc sử dụng thành công! Số dư đã được cập nhật.";
        //     return RedirectToAction("Home");
        // }
        [HttpPost]
// filepath: /Users/thinguyen/TL_ky_3/lap trinh.NET/Project/Controllers/KhachHangController.cs
public IActionResult KetThucSuDung()
{   
    string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? string.Empty;

    if (string.IsNullOrEmpty(maNguoiDung))
    {
        TempData["Error"] = "Không xác định được người dùng.";
        return RedirectToAction("DangNhap", "Home");
    }

    var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == maNguoiDung);
    if (nguoiDung == null)
    {
        TempData["Error"] = "Không tìm thấy thông tin người dùng.";
        return RedirectToAction("Home");
    }

    var suDungMay = _context.SuDungMays
        .Include(sdm => sdm.MaMayNavigation)
        .FirstOrDefault(sdm => sdm.MaNguoiDung == maNguoiDung && sdm.ThoiGianKetThuc == null);

    if (suDungMay == null)
    {
        TempData["Error"] = "Không tìm thấy thông tin sử dụng máy.";
        return RedirectToAction("Home");
    }

    // Tính toán thời gian sử dụng và chi phí
    var thoiGianSuDung = (DateTime.Now - suDungMay.ThoiGianBatDau).TotalMinutes;
    var donGia = suDungMay.MaMayNavigation?.DonGia ?? 0;
    var chiPhi = (decimal)(thoiGianSuDung / 60) * donGia;

    // Cập nhật thông tin sử dụng máy
    suDungMay.ThoiGianKetThuc = DateTime.Now;
    suDungMay.TongTien = chiPhi;
    suDungMay.TongThoiGian = (decimal)thoiGianSuDung;

    // Kiểm tra nếu số dư nhỏ hơn chi phí
    if ((nguoiDung.SoDu ?? 0) < chiPhi)
    {
        // Tự động kết thúc sử dụng
        var may = suDungMay.MaMayNavigation;
        if (may != null)
        {
            may.TrangThai = "Sẵn sàng";
            _context.MayTinhs.Update(may);
        }

        _context.SuDungMays.Update(suDungMay);
        _context.SaveChanges();

        TempData["Error"] = "Số dư không đủ. Hệ thống đã tự động kết thúc sử dụng.";
        return RedirectToAction("Home");
    }

    // Trừ số dư và kết thúc sử dụng
    nguoiDung.SoDu = (nguoiDung.SoDu ?? 0) - chiPhi;

    var mayKetThuc = suDungMay.MaMayNavigation;
    if (mayKetThuc != null)
    {
        mayKetThuc.TrangThai = "Sẵn sàng";
        _context.MayTinhs.Update(mayKetThuc);
    }

    _context.NguoiDungs.Update(nguoiDung);
    _context.SuDungMays.Update(suDungMay);
    _context.SaveChanges();

    TempData["Success"] = "Kết thúc sử dụng thành công! Số dư đã được cập nhật.";
    return RedirectToAction("Home");
}

        // get lich su

            [HttpGet]
            public IActionResult LichSu()
            {
                string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? string.Empty;

                if (string.IsNullOrEmpty(maNguoiDung))
                {
                    TempData["Error"] = "Không xác định được người dùng.";
                    return RedirectToAction("DangNhap", "Home");
                }

                var lichSuSuDung = _context.SuDungMays
                    .Include(sdm => sdm.MaMayNavigation)
                    .Where(sdm => sdm.MaNguoiDung == maNguoiDung)
                    .OrderByDescending(sdm => sdm.ThoiGianBatDau)
                    .Select(sdm => new LichSuSuDungViewModel
                    {
                        TenMay = sdm.MaMayNavigation.TenMay,
                        ThoiGianBatDau = sdm.ThoiGianBatDau,
                        ThoiGianKetThuc = sdm.ThoiGianKetThuc,
                        TongThoiGian = sdm.TongThoiGian,
                        TongTien = sdm.TongTien
                    })
                    .ToList();

                return View(lichSuSuDung);
            }
        // GET: nap tien
        [HttpGet]
        public IActionResult NapTien()
        {
            // Trả về view hiển thị thông tin nạp tiền
            return View();
        }

        [HttpPost]
        public IActionResult NapTien(decimal soTien)
        {
            string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(maNguoiDung))
            {
                TempData["Error"] = "Không xác định được người dùng.";
                return RedirectToAction("DangNhap", "Home");
            }

            var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == maNguoiDung);
            if (nguoiDung == null)
            {
                TempData["Error"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("Home");
            }

            // Cộng số tiền nạp vào số dư hiện tại
            nguoiDung.SoDu = (nguoiDung.SoDu ?? 0) + soTien;

            _context.NguoiDungs.Update(nguoiDung);
            _context.SaveChanges();
              TempData["Success"] = $"Nạp tiền thành công! Số tiền đã nạp: {soTien:N0} VNĐ.";

            return RedirectToAction("Home");
        }

        // get thông tin
        [HttpGet]
        public IActionResult GetThongTinSuDung()
        {
            string maNguoiDung = User.FindFirst("MaNguoiDung")?.Value ?? string.Empty;

            if (string.IsNullOrEmpty(maNguoiDung))
            {
                Console.WriteLine(">>> Không xác định được người dùng."); // Log lỗi
                return Json(new { success = false, message = "Không xác định được người dùng." });
            }

            var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == maNguoiDung);
            if (nguoiDung == null)
            {
                Console.WriteLine(">>> Không tìm thấy thông tin người dùng."); // Log lỗi
                return Json(new { success = false, message = "Không tìm thấy thông tin người dùng." });
            }

            var suDungMay = _context.SuDungMays
                .Include(sdm => sdm.MaMayNavigation)
                .FirstOrDefault(sdm => sdm.MaNguoiDung == maNguoiDung && sdm.ThoiGianKetThuc == null);

            // if (suDungMay == null)
            // {
            //     return Json(new { success = false, message = "Không tìm thấy thông tin sử dụng máy." });
            // }

            var thoiGianSuDung = (DateTime.Now - suDungMay.ThoiGianBatDau).TotalMinutes;
            var donGia = suDungMay.MaMayNavigation?.DonGia ?? 0;
            var chiPhi = (decimal)(thoiGianSuDung / 60) * donGia;
            var soDuHienTai = (nguoiDung.SoDu ?? 0) - chiPhi;

              if (soDuHienTai < 0)
              {
                  // Tự động kết thúc phiên sử dụng
                  suDungMay.ThoiGianKetThuc = DateTime.Now;
                  suDungMay.TongTien = chiPhi;
                  suDungMay.TongThoiGian = (decimal)thoiGianSuDung;
                  nguoiDung.SoDu = 0; // Đặt số dư về 0
                
                  // Cập nhật trạng thái máy về "Sẵn sàng"
                  var may = suDungMay.MaMayNavigation;
                  if (may != null)
                  {
                      may.TrangThai = "Sẵn sàng";
                      _context.MayTinhs.Update(may);
                  }
                  _context.NguoiDungs.Update(nguoiDung);
                  _context.SuDungMays.Update(suDungMay);
                  _context.SaveChanges();

                  return Json(new { success = false, message = "Số dư không đủ. Phiên sử dụng đã kết thúc." });
              }
       
            return Json(new
            {
                success = true,
                thoiGianSuDung = thoiGianSuDung,
                chiPhi = Math.Round(chiPhi, 2),
                soDuHienTai = Math.Round(soDuHienTai, 2), // Làm tròn đến 2 chữ số thập phân
            });
        }
    }
}