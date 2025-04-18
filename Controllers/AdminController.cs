using Microsoft.AspNetCore.Mvc;
using Project.Data;
using Project.Models.Admin;
using Project.Models.Home;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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
        public IActionResult QuanLyNguoiDung()
        {
            // Lấy danh sách người dùng từ cơ sở dữ liệu và sắp xếp Admin lên đầu
            var users = _context.NguoiDungs
                .Where(nd => nd.TrangThai == "Hoạt động") // Chỉ lấy người dùng có trạng thái "Hoạt động"
                .OrderByDescending(nd => nd.Role == "Admin") // Sắp xếp Admin lên đầu
                .ThenBy(nd => nd.HoTen) // Sắp xếp theo Họ tên (nếu cần)
                .Select(nd => new QuanLyNguoiDungViewModel
                {
                    MaNguoiDung = nd.MaNguoiDung,
                    TenDangNhap = nd.TenDangNhap,
                    HoTen = nd.HoTen,
                    SoDienThoai = nd.SoDienThoai,
                    Role = nd.Role
                })
                .ToList();
        
            return View(users);
        }

        [HttpGet]
        public IActionResult ThemNguoiDung()
        {
            return View();
        }
        
        [HttpPost]
        public IActionResult ThemNguoiDung(DangKyViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Kiểm tra xem tên đăng nhập đã tồn tại chưa
                var existingUserByUsername = _context.NguoiDungs
                    .FirstOrDefault(nd => nd.TenDangNhap == model.Username);
        
                if (existingUserByUsername != null)
                {
                    ModelState.AddModelError("Username", "Tên đăng nhập đã tồn tại.");
                    return View(model);
                }
        
                // Kiểm tra xem số điện thoại đã tồn tại chưa
                var existingUserByPhone = _context.NguoiDungs
                    .FirstOrDefault(nd => nd.SoDienThoai == model.PhoneNumber);
        
                if (existingUserByPhone != null)
                {
                    ModelState.AddModelError("PhoneNumber", "Số điện thoại đã được sử dụng.");
                    return View(model);
                }

                // Kiểm tra số điện thoại có đủ 10 số không
                if (model.PhoneNumber == null || model.PhoneNumber.Length != 10 || !model.PhoneNumber.All(char.IsDigit))
                {
                    ModelState.AddModelError("PhoneNumber", "Số điện thoại phải có đúng 10 chữ số.");
                    return View(model);
                }
        
                // Lấy ID lớn nhất hiện có trong cơ sở dữ liệu
                var lastUser = _context.NguoiDungs
                    .OrderByDescending(nd => nd.MaNguoiDung)
                    .FirstOrDefault();
        
                string newId;
                if (lastUser == null)
                {
                    // Nếu chưa có người dùng nào, bắt đầu từ 0000000001
                    newId = "0000000001";
                }
                else
                {
                    // Tăng ID lên 1
                    newId = (long.Parse(lastUser.MaNguoiDung) + 1).ToString("D10");
                }
        
                // Tạo người dùng mới
                var nguoiDungMoi = new NguoiDung
                {
                    MaNguoiDung = newId, // Gán ID mới
                    HoTen = model.FullName ?? string.Empty,
                    TenDangNhap = model.Username ?? string.Empty,
                    MatKhau = model.Password ?? string.Empty,
                    SoDienThoai = model.PhoneNumber,
                    Role = model.Role ?? string.Empty,
                    NgayTaoTaiKhoan = DateTime.Now, // Gán ngày tạo tài khoản
                    TrangThai = "Hoạt động" // Gán trạng thái mặc định
                };
        
                _context.NguoiDungs.Add(nguoiDungMoi);
                _context.SaveChanges();
        
                // Đăng ký thành công
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
        public IActionResult QuanLyMayTinh()
        {
            // Lấy danh sách máy tính từ cơ sở dữ liệu
            var mayTinhList = _context.MayTinhs
                .Where(mt => mt.TrangThai != "Đã xóa") // Chỉ lấy máy tính không bị xóa
                .OrderBy(mt => mt.MaMay) // Sắp xếp theo vị trí
                .Select(mt => new QuanLyMayTinhViewModel
                {
                    MaMay = mt.MaMay,
                    TenMay = mt.TenMay,
                    TrangThai = mt.TrangThai,
                    DonGia = (int)mt.DonGia,
                    MoTa = mt.MoTa
                })
                .ToList();
        
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
        public IActionResult XoaMayTinh(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                TempData["Error"] = "ID máy tính không hợp lệ.";
                return RedirectToAction("QuanLyMayTinh");
            }
        
            var mayTinh = _context.MayTinhs.FirstOrDefault(mt => mt.MaMay == id);
            if (mayTinh == null)
            {
                TempData["Error"] = "Không tìm thấy máy tính.";
                return RedirectToAction("QuanLyMayTinh");
            }
        
            if (mayTinh.TrangThai == "Hoạt động")
            {
                TempData["Error"] = "Máy tính đang được sử dụng. Không thể xóa.";
                return RedirectToAction("QuanLyMayTinh");
            }
        
            var hasHistory = _context.SuDungMays.Any(sdm => sdm.MaMay == id);
            if (hasHistory)
            {
                TempData["Warning"] = "Máy tính này có lịch sử sử dụng. Nên đặt trạng thái máy về 'Bảo trì' thay vì xóa.";
            }
        
            mayTinh.TrangThai = "Đã xóa";
            mayTinh.ThoiGianXoa = DateTime.Now;
        
            _context.SaveChanges();
        
            TempData["Message"] = "Máy tính đã được chuyển sang trạng thái 'Đã xóa'.";
            return RedirectToAction("QuanLyMayTinh");
        }
    }
}