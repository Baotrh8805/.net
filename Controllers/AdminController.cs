using Microsoft.AspNetCore.Mvc;
using Project.Data;
using Project.Models.Admin;
using Project.Models.Home;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Project.Models.KhachHang;


namespace Project.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ProjetcNetQuanLyMayTinhContext _context;

        public AdminController(ProjetcNetQuanLyMayTinhContext context)
        {
            _context = context;
        }

        public IActionResult Home()
        {
            // Lấy MaNguoiDung từ Claims
            var maNguoiDung = User.FindFirst("MaNguoiDung")?.Value;
        
            // Truy vấn cơ sở dữ liệu để lấy thông tin người dùng
            var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == maNguoiDung);
        
            // Nếu không tìm thấy người dùng, đặt giá trị mặc định
            var hoTen = nguoiDung?.HoTen ?? "Không xác định";
        
            // Lấy dữ liệu thống kê
            var soQuanTriVien = _context.NguoiDungs.Count(nd => nd.Role == "Admin");
            var soNguoiDung = _context.NguoiDungs.Count(nd => nd.Role == "Khách");
            var soMayTinh = _context.MayTinhs.Count();
            var tongDoanhThu = _context.SuDungMays.Sum(sdm => sdm.TongTien ?? 0);
        
            // Tạo ViewModel
            var viewModel = new AdminHomeViewModel
            {
                HoTen = hoTen,
                SoNguoiDung = soNguoiDung,
                SoQuanTriVien = soQuanTriVien,
                SoMayTinh = soMayTinh,
                TongDoanhThu = tongDoanhThu
            };
        
            return View(viewModel);
        }

        [HttpGet]
        public IActionResult QuanLyNguoiDung(string search)
        {
            // Lấy danh sách người dùng từ cơ sở dữ liệu
            var userList = _context.NguoiDungs
                .Where(nd => (string.IsNullOrEmpty(search) || 
                             nd.TenDangNhap.Contains(search) || 
                             nd.HoTen.Contains(search) || 
                             (nd.SoDienThoai != null && nd.SoDienThoai.Contains(search))) && nd.TrangThai != "Đã xóa")
                .Select(nd => new QuanLyNguoiDungViewModel
                {
                    MaNguoiDung = nd.MaNguoiDung,
                    TenDangNhap = nd.TenDangNhap,
                    HoTen = nd.HoTen,
                    SoDienThoai = nd.SoDienThoai,
                    Role = nd.Role
                })
                .ToList();
        
            // Lưu từ khóa tìm kiếm vào ViewData để hiển thị lại trên giao diện
            ViewData["Search"] = search;
        
            return View(userList);
        }        
        
       [HttpGet]
public IActionResult ThemNguoiDung()
{
    var model = new ThemNguoiDungViewModel();
    Console.WriteLine("Model initialized: " + (model != null));
    return View(model);
}

     [HttpPost]
public IActionResult ThemNguoiDung(ThemNguoiDungViewModel model)
{
    if (ModelState.IsValid)
    {
        // Kiểm tra tên đăng nhập đã tồn tại (bỏ qua tài khoản có trạng thái "Đã xóa")
        var existingUser = _context.NguoiDungs
            .FirstOrDefault(nd => nd.TenDangNhap == model.TenDangNhap && nd.TrangThai != "Đã xóa");

        if (existingUser != null)
        {
            ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
            return View(model);
        }

        // Kiểm tra số điện thoại đã tồn tại (bỏ qua tài khoản có trạng thái "Đã xóa")
        var existingPhone = _context.NguoiDungs
            .FirstOrDefault(nd => nd.SoDienThoai == model.SoDienThoai && nd.TrangThai != "Đã xóa");

        if (existingPhone != null)
        {
            ModelState.AddModelError("SoDienThoai", "Số điện thoại đã được sử dụng.");
            return View(model);
        }

        // Tạo ID mới cho người dùng
        var lastUser = _context.NguoiDungs.OrderByDescending(nd => nd.MaNguoiDung).FirstOrDefault();
        string newId = lastUser == null ? "0000000001" : (long.Parse(lastUser.MaNguoiDung) + 1).ToString("D10");

        // Thêm người dùng mới
        var newUser = new NguoiDung
        {
            MaNguoiDung = newId,
            TenDangNhap = model.TenDangNhap,
            MatKhau = model.MatKhau,
            HoTen = model.HoTen,
            SoDienThoai = model.SoDienThoai,
            Role = model.Role,
            TrangThai = "Hoạt động",
            NgayTaoTaiKhoan = DateTime.Now
        };

        _context.NguoiDungs.Add(newUser);
        _context.SaveChanges();

        TempData["Success"] = "Thêm người dùng thành công.";
        return RedirectToAction("QuanLyNguoiDung");
    }

    return View(model);
}
        
        
        [HttpGet]
        public IActionResult ChinhSuaNguoiDung(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID không hợp lệ.";
                return RedirectToAction("QuanLyNguoiDung");
            }
        
            // Tìm người dùng theo ID
            var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == id);
        
            if (nguoiDung == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("QuanLyNguoiDung");
            }
        
            // Tạo ViewModel để truyền dữ liệu vào View
            var model = new ChinhSuaNguoiDungViewModel
            {
                MaNguoiDung = nguoiDung.MaNguoiDung,
                TenDangNhap = nguoiDung.TenDangNhap,
                MatKhau = nguoiDung.MatKhau,
                HoTen = nguoiDung.HoTen,
                SoDienThoai = nguoiDung.SoDienThoai,
                Role = nguoiDung.Role
            };
        
            return View(model);
        }
        
        [HttpPost]
        public IActionResult ChinhSuaNguoiDung(ChinhSuaNguoiDungViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm người dùng theo ID
                var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == model.MaNguoiDung);
        
                if (nguoiDung == null)
                {
                    Console.WriteLine($"Không tìm thấy người dùng với MaNguoiDung: {model.MaNguoiDung}");
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction("QuanLyNguoiDung");
                }
        
                // Kiểm tra tên đăng nhập không bị trùng (ngoại trừ chính người dùng đang chỉnh sửa)
                var existingUserByUsername = _context.NguoiDungs
                    .FirstOrDefault(nd => nd.TenDangNhap == model.TenDangNhap && nd.MaNguoiDung != model.MaNguoiDung);
        
                if (existingUserByUsername != null)
                {
                    ModelState.AddModelError("TenDangNhap", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }
        
                // Kiểm tra số điện thoại không bị trùng (ngoại trừ chính người dùng đang chỉnh sửa)
                var existingUserByPhone = _context.NguoiDungs
                    .FirstOrDefault(nd => nd.SoDienThoai == model.SoDienThoai && nd.MaNguoiDung != model.MaNguoiDung);
        
                if (existingUserByPhone != null)
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại đã được sử dụng.");
                    return View(model);
                }
        
                // Kiểm tra số điện thoại có đủ 10 số và chỉ chứa chữ số
                if (string.IsNullOrEmpty(model.SoDienThoai) || model.SoDienThoai.Length != 10 || !model.SoDienThoai.All(char.IsDigit))
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại phải có đúng 10 chữ số.");
                    return View(model);
                }
        
                // Kiểm tra các trường không được để trống
                if (string.IsNullOrEmpty(model.TenDangNhap) || string.IsNullOrEmpty(model.HoTen))
                {
                    ModelState.AddModelError("", "Các trường không được để trống.");
                    return View(model);
                }
        
                // Cập nhật thông tin người dùng
                nguoiDung.TenDangNhap = model.TenDangNhap;
                nguoiDung.MatKhau = model.MatKhau ?? string.Empty;
                nguoiDung.HoTen = model.HoTen;
                nguoiDung.SoDienThoai = model.SoDienThoai;
                nguoiDung.Role = model.Role;
        
                // Lưu thay đổi vào cơ sở dữ liệu
                Console.WriteLine("Đang lưu thay đổi vào cơ sở dữ liệu...");
                _context.SaveChanges();
                Console.WriteLine("Lưu thay đổi thành công.");
        
                TempData["Message"] = "Cập nhật thông tin người dùng thành công.";
                return RedirectToAction("QuanLyNguoiDung");
            }
        
            return View(model);
        }

        [HttpGet]
        public IActionResult XoaTaiKhoan(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID người dùng không hợp lệ.";
                return RedirectToAction("QuanLyNguoiDung");
            }
        
            // Tìm người dùng theo ID
            var nguoiDung = _context.NguoiDungs.FirstOrDefault(nd => nd.MaNguoiDung == id);
        
            if (nguoiDung == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("QuanLyNguoiDung");
            }
        
            // Kiểm tra nếu người dùng đang sử dụng máy
            var suDungMay = _context.SuDungMays.FirstOrDefault(sdm => sdm.MaNguoiDung == id && sdm.ThoiGianKetThuc == null);
            if (suDungMay != null && nguoiDung.SoDu > 0)
            {
                TempData["Error"] = "Người dùng đang sử dụng máy và còn số dư. Không thể xóa.";
                return RedirectToAction("QuanLyNguoiDung");
            }
        
            // Kiểm tra nếu người dùng từng sử dụng máy
            var daSuDungMay = _context.SuDungMays.Any(sdm => sdm.MaNguoiDung == id);
            if (daSuDungMay)
            {
                TempData["Warning"] = "Người dùng này đã từng sử dụng máy.";
            }
        
            // Thay đổi trạng thái của người dùng
            nguoiDung.TrangThai = "Đã xóa";
        
            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();
        
            TempData["Message"] = "Tài khoản đã được chuyển sang trạng thái 'Đã xóa'.";
            return RedirectToAction("QuanLyNguoiDung");
        }

        [HttpGet]
        public IActionResult QuanLyMayTinh(string search)
        {
            // Lấy danh sách máy tính từ cơ sở dữ liệu
                var mayTinhList = _context.MayTinhs
                .Where(mt => (string.IsNullOrEmpty(search) || 
                             mt.TenMay.Contains(search) || 
                             mt.MaMay.Contains(search) || 
                             (mt.MoTa != null && mt.MoTa.Contains(search))) && mt.TrangThai != "Đã xóa") // Tìm kiếm theo tên, mã máy, hoặc mô tả
                .Select(mt => new QuanLyMayTinhViewModel
                {
                    MaMay = mt.MaMay,
                    TenMay = mt.TenMay,
                    TrangThai = mt.TrangThai,
                    DonGia = (double) mt.DonGia,
                    MoTa = mt.MoTa
                })
                .ToList();
            // Lưu từ khóa tìm kiếm vào ViewData để hiển thị lại trên giao diện
            ViewData["Search"] = search;
            return View(mayTinhList);
        }

        [HttpGet]
        public IActionResult ThemMayTinh()
        {
            var model = new ThemMayTinhViewModel(); // Khởi tạo model rỗng
            return View(model);
        }

        [HttpPost]
        public IActionResult ThemMayTinh(ThemMayTinhViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên máy đã tồn tại hay chưa
                var existingMayTinh = _context.MayTinhs.FirstOrDefault(mt => mt.TenMay == model.TenMay);
                if (existingMayTinh != null)
                {
                    ModelState.AddModelError("TenMay", "Tên máy đã tồn tại.");
                    return View(model);
                }
        
                // Lấy ID lớn nhất hiện có trong cơ sở dữ liệu
                var lastMayTinh = _context.MayTinhs
                    .OrderByDescending(mt => mt.MaMay)
                    .FirstOrDefault();
        
                string newId;
                if (lastMayTinh == null)
                {
                    // Nếu chưa có máy tính nào, bắt đầu từ 0000000001
                    newId = "0000000001";
                }
                else
                {
                    // Tăng ID lên 1
                    newId = (long.Parse(lastMayTinh.MaMay) + 1).ToString("D10");
                }
        
                // Thêm máy tính mới
                var newMayTinh = new MayTinh
                {
                    MaMay = newId, // Gán ID tự động
                    TenMay = model.TenMay ?? string.Empty,
                    TrangThai = "Sẵn sàng", // Trạng thái mặc định
                    DonGia = model.DonGia ?? 0,
                    MoTa = model.MoTa,
                    ThoiGianTao = DateTime.Now // Ngày tạo máy tính
                };
        
                _context.MayTinhs.Add(newMayTinh);
                _context.SaveChanges();
        
                TempData["Message"] = "Thêm máy tính thành công.";
                return RedirectToAction("QuanLyMayTinh");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult ChinhSuaMayTinh(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID máy tính không hợp lệ.";
                return RedirectToAction("QuanLyMayTinh");
            }
        
            // Tìm máy tính theo ID
            var mayTinh = _context.MayTinhs.FirstOrDefault(mt => mt.MaMay == id);
        
            if (mayTinh == null)
            {
                TempData["Error"] = "Không tìm thấy máy tính.";
                return RedirectToAction("QuanLyMayTinh");
            }
        
            // Tạo ViewModel để truyền dữ liệu vào View
            var model = new ChinhSuaMayTinhViewModel
            {
                MaMay = mayTinh.MaMay,
                TenMay = mayTinh.TenMay,
                TrangThai = mayTinh.TrangThai,
                DonGia = (double) mayTinh.DonGia,
                MoTa = mayTinh.MoTa
            };
        
            return View(model);
        }

        [HttpPost]
        public IActionResult ChinhSuaMayTinh(ChinhSuaMayTinhViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tìm máy tính theo ID
                var mayTinh = _context.MayTinhs.FirstOrDefault(mt => mt.MaMay == model.MaMay);
        
                if (mayTinh == null)
                {
                    TempData["Error"] = "Không tìm thấy máy tính.";
                    return RedirectToAction("QuanLyMayTinh");
                }
        
                // Kiểm tra nếu máy tính đang được sử dụng
                var suDungMay = _context.SuDungMays.FirstOrDefault(sdm => sdm.MaMay == model.MaMay && sdm.ThoiGianKetThuc == null);
                if (suDungMay != null)
                {
                    TempData["Error"] = "Máy tính đang được sử dụng. Không thể chỉnh sửa.";
                    return RedirectToAction("QuanLyMayTinh");
                }
        
                // Kiểm tra tên máy không bị trùng (ngoại trừ chính máy tính đang chỉnh sửa)
                var existingMayTinh = _context.MayTinhs.FirstOrDefault(mt => mt.TenMay == model.TenMay && mt.MaMay != model.MaMay);
                if (existingMayTinh != null)
                {
                    ModelState.AddModelError("TenMay", "Tên máy đã tồn tại.");
                    return View(model);
                }
        
                // Cập nhật thông tin máy tính
                mayTinh.TenMay = model.TenMay ?? string.Empty;
                mayTinh.TrangThai = model.TrangThai;
                mayTinh.DonGia = (decimal)model.DonGia;
                mayTinh.MoTa = model.MoTa;
        
                // Lưu thay đổi vào cơ sở dữ liệu
                _context.SaveChanges();
        
                TempData["Message"] = "Cập nhật thông tin máy tính thành công.";
                return RedirectToAction("QuanLyMayTinh");
            }
        
            return View(model);
        }

       

        [HttpGet]
        public IActionResult ThongKeTheoMay(string search)
        {
            var mayTinhList = _context.MayTinhs
                .Where(mt => string.IsNullOrEmpty(search) || mt.TenMay.Contains(search) || mt.MaMay.Contains(search))
                .Select(mt => new
                {
                    mt.MaMay,
                    mt.TenMay,
                    mt.TrangThai,
                    mt.MoTa,
                    SuDungMays = _context.SuDungMays
                        .Where(sdm => sdm.MaMay == mt.MaMay)
                        .ToList() // chuyển sang client-side xử lý
                })
                .AsEnumerable()
                .Select(item => new ThongKeViewModel
                {
                    MaMay = item.MaMay,
                    TenMay = item.TenMay,
                    TrangThai = item.TrangThai,
                    MoTa = item.MoTa,
                    SoLanSuDung = item.SuDungMays.Count,
                    TongGioSuDung = item.SuDungMays
                        .Where(sdm => sdm.ThoiGianKetThuc.HasValue)
                        .Sum(sdm => (sdm.ThoiGianKetThuc.Value - sdm.ThoiGianBatDau).TotalHours),
                    TongDoanhThu = item.SuDungMays.Sum(sdm => sdm.TongTien ?? 0)
                })
                .OrderByDescending(item => item.TongDoanhThu) // Sắp xếp theo doanh thu giảm dần
                .ToList();

            ViewData["Search"] = search;
            return View(mayTinhList);
        }
    
        [HttpGet]
        public IActionResult ThongKeChiTietMay(string maMay)
        {
            if (string.IsNullOrEmpty(maMay))
            {
                TempData["Error"] = "Mã máy không hợp lệ.";
                return RedirectToAction("ThongKeTheoMay");
            }
        
            var mayTinh = _context.MayTinhs.FirstOrDefault(mt => mt.MaMay == maMay);
            if (mayTinh == null)
            {
                TempData["Error"] = "Không tìm thấy máy tính.";
                return RedirectToAction("ThongKeTheoMay");
            }
            
            var chiTiet = _context.SuDungMays
            .Where(sdm => sdm.MaMay == maMay)
            .Select(sdm => new ThongKeViewModel
            {
                MaMay = maMay,
                TenMay = mayTinh.TenMay,
                ThoiGianBatDau = sdm.ThoiGianBatDau,
                ThoiGianKetThuc = sdm.ThoiGianKetThuc,
                TongDoanhThu = sdm.TongTien ?? 0,
                TenNguoiDung = _context.NguoiDungs
                    .Where(nd => nd.MaNguoiDung == sdm.MaNguoiDung)
                    .Select(nd => nd.HoTen)
                    .FirstOrDefault(),
                SoDienThoai = _context.NguoiDungs
                    .Where(nd => nd.MaNguoiDung == sdm.MaNguoiDung)
                    .Select(nd => nd.SoDienThoai)
                    .FirstOrDefault()
            })
            .ToList();
        
            ViewData["TenMay"] = mayTinh.TenMay;
            ViewData["MaMay"] = maMay;
        
            return View(chiTiet);
        }
         
        [HttpGet]
        public IActionResult ThongKeTheoThoiGian(DateTime? startDate, DateTime? endDate)
        {
            // Nếu không chọn ngày, mặc định lấy toàn bộ dữ liệu
            if (!startDate.HasValue || !endDate.HasValue)
            {
                startDate = new DateTime(2025, 1, 1); // Giá trị nhỏ nhất hợp lệ cho SQL Server
                endDate = DateTime.Now; // Mặc định là ngày hiện tại
            }
        
            // Lấy danh sách thống kê theo khoảng thời gian
            var thongKeList = _context.SuDungMays
                .Where(sdm => sdm.ThoiGianBatDau >= startDate && sdm.ThoiGianKetThuc <= endDate)
                .GroupBy(sdm => sdm.MaMay) // Nhóm theo mã máy
                .Select(group => new ThongKeViewModel
                {
                    MaMay = group.Key,
                    TenMay = _context.MayTinhs
                        .Where(mt => mt.MaMay == group.Key)
                        .Select(mt => mt.TenMay)
                        .FirstOrDefault(),
                    TongDoanhThu = group.Sum(sdm => sdm.TongTien ?? 0) // Tính tổng doanh thu trong khoảng thời gian
                })
                .OrderByDescending(sdm => sdm.TongDoanhThu) // Sắp xếp theo doanh thu giảm dần
                .ToList();
        
            // Truyền dữ liệu ngày bắt đầu và kết thúc để hiển thị lại trên giao diện
            ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
        
            return View(thongKeList);
        }

        [HttpGet]
        public IActionResult ThongKeChiTietThoiGian(string maMay, DateTime? startDate, DateTime? endDate)
        {
            if (string.IsNullOrEmpty(maMay))
            {
                TempData["Error"] = "Mã máy không hợp lệ.";
                return RedirectToAction("ThongKeTheoMay");
            }
        
            var mayTinh = _context.MayTinhs.FirstOrDefault(mt => mt.MaMay == maMay);
            if (mayTinh == null)
            {
                TempData["Error"] = "Không tìm thấy máy tính.";
                return RedirectToAction("ThongKeTheoMay");
            }
        
            // Nếu không chọn ngày, đặt giá trị mặc định
            if (!startDate.HasValue)
            {
                startDate = new DateTime(1753, 1, 1); // Giá trị nhỏ nhất hợp lệ cho SQL Server
            }
            if (!endDate.HasValue)
            {
                endDate = DateTime.Now; // Mặc định là ngày hiện tại
            }
        
            // Lấy danh sách chi tiết sử dụng máy theo khoảng thời gian
            var chiTiet = _context.SuDungMays
                .Where(sdm => sdm.MaMay == maMay && sdm.ThoiGianBatDau >= startDate && sdm.ThoiGianKetThuc <= endDate)
                .Select(sdm => new ThongKeViewModel
                {
                    MaMay = maMay,
                    TenMay = mayTinh.TenMay,
                    ThoiGianBatDau = sdm.ThoiGianBatDau,
                    ThoiGianKetThuc = sdm.ThoiGianKetThuc,
                    TongDoanhThu = sdm.TongTien ?? 0,
                    TenNguoiDung = _context.NguoiDungs
                        .Where(nd => nd.MaNguoiDung == sdm.MaNguoiDung)
                        .Select(nd => nd.HoTen)
                        .FirstOrDefault(),
                    SoDienThoai = _context.NguoiDungs
                        .Where(nd => nd.MaNguoiDung == sdm.MaNguoiDung)
                        .Select(nd => nd.SoDienThoai)
                        .FirstOrDefault()
                })
                .OrderByDescending(sdm => sdm.ThoiGianBatDau) // Sắp xếp theo thời gian bắt đầu giảm dần
                .ToList();
        
            // Truyền dữ liệu ngày bắt đầu và kết thúc để hiển thị lại trên giao diện
            ViewData["StartDate"] = startDate.Value.ToString("yyyy-MM-dd");
            ViewData["EndDate"] = endDate.Value.ToString("yyyy-MM-dd");
            ViewData["TenMay"] = mayTinh.TenMay;
            ViewData["MaMay"] = maMay;
        
            return View(chiTiet);
        }
[HttpGet]
        public IActionResult XoaNguoiDung(string userId)
        {
            Console.WriteLine($"MaNguoiDung nhận được 123: {userId}");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "ID người dùng không hợp lệ.";
                return RedirectToAction("QuanLyNguoiDung");
            }
        
            var user = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("QuanLyNguoiDung");
            }
            // Kiểm tra nếu người dùng có dữ liệu liên quan
            var hasRelatedData = _context.SuDungMays.Any(sdm => sdm.MaNguoiDung == userId);

            // Tạo ViewModel để truyền dữ liệu sang View
            var viewModel = new XoaNguoiDungViewModel
            {
                MaNguoiDung = user.MaNguoiDung,
                HoTen = user.HoTen,
                SoDienThoai = user.SoDienThoai,
                CoDuLieuLienQuan = hasRelatedData,
                SoDu = user.SoDu ?? 0
            };

            return View(viewModel);
        }

        [HttpPost]
        public IActionResult XoaNguoiDungConfirm (String userId)
        {
            Console.WriteLine($"MaNguoiDung nhận được: {userId}");
            if (string.IsNullOrEmpty(userId))
            {
                TempData["Error"] = "ID người dùng không hợp lệ.";
                return RedirectToAction("QuanLyNguoiDung");
            }
        
            var user = _context.NguoiDungs.FirstOrDefault(u => u.MaNguoiDung == userId);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng.";
                return RedirectToAction("QuanLyNguoiDung");
            }
        
            // Đánh dấu người dùng là đã xóa
            user.TrangThai = "Đã xóa";
            user.NgayXoaTaiKhoan = DateTime.Now; // Ghi lại thời gian xóa tài khoản
    
            // Lưu thay đổi vào cơ sở dữ liệu
            _context.SaveChanges();
    
            TempData["Message"] = "Xóa người dùng thành công.";
            return RedirectToAction("QuanLyNguoiDung");
        }
    }
}