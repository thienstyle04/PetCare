using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Petcare_web.Models.DTO;
using System.Net.Http.Headers;
using System.Text;

namespace Petcare_web.Controllers.Khachhang
{
    [Authorize(Roles = "KHACHHANG")]
    public class CustomerProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;
        private readonly ILogger<CustomerProfileController> _logger;

        public CustomerProfileController(
            IHttpClientFactory httpClientFactory, 
            IConfiguration configuration,
            ILogger<CustomerProfileController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7053";
            _logger = logger;
        }

        // GET: CustomerProfile/Index - Hiển thị thông tin cá nhân
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                var token = HttpContext.Session.GetString("JwtToken");

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy thông tin khách hàng
                var response = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{userId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Không thể tải thông tin khách hàng. Status: {StatusCode}", response.StatusCode);
                    TempData["ErrorMessage"] = "Không thể tải thông tin cá nhân";
                    return View();
                }

                var json = await response.Content.ReadAsStringAsync();
                var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(json);

                if (khachHang == null)
                {
                    _logger.LogError("Dữ liệu khách hàng null");
                    TempData["ErrorMessage"] = "Không tìm thấy thông tin khách hàng";
                    return View();
                }

                _logger.LogInformation("Hiển thị profile khách hàng ID: {CustomerId}", khachHang.MaKhachHang);
                return View(khachHang);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải profile");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải thông tin cá nhân";
                return View();
            }
        }

        // GET: CustomerProfile/Edit - Form chỉnh sửa thông tin
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                var token = HttpContext.Session.GetString("JwtToken");

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{userId}");
                
                if (!response.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không thể tải thông tin để chỉnh sửa";
                    return RedirectToAction("Index");
                }

                var json = await response.Content.ReadAsStringAsync();
                var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(json);

                return View(khachHang);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form chỉnh sửa");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi";
                return RedirectToAction("Index");
            }
        }

        // POST: CustomerProfile/Edit - Cập nhật thông tin
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(KhachHangDTO model)
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Validate
                if (string.IsNullOrWhiteSpace(model.HoTen))
                {
                    ModelState.AddModelError("HoTen", "Họ tên không được để trống");
                }
                if (string.IsNullOrWhiteSpace(model.SoDienThoai))
                {
                    ModelState.AddModelError("SoDienThoai", "Số điện thoại không được để trống");
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Tạo DTO để update
                var updateDto = new
                {
                    HoTen = model.HoTen,
                    SoDienThoai = model.SoDienThoai,
                    DiaChi = model.DiaChi,
                    NgaySinh = model.NgaySinh
                };

                var jsonContent = JsonConvert.SerializeObject(updateDto);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Cập nhật thông tin khách hàng ID: {CustomerId}", model.MaKhachHang);

                var response = await client.PutAsync(
                    $"{_apiBaseUrl}/api/Khachhang/UpdateKhachHang/{model.MaKhachHang}", 
                    content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Cập nhật thành công khách hàng ID: {CustomerId}", model.MaKhachHang);
                    TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Cập nhật thất bại. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Không thể cập nhật thông tin. Vui lòng thử lại.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi cập nhật thông tin");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi cập nhật";
                return View(model);
            }
        }

        // GET: CustomerProfile/ChangePassword - Form đổi mật khẩu
        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        // POST: CustomerProfile/ChangePassword - Xử lý đổi mật khẩu
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordDTO model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                if (model.NewPassword != model.ConfirmPassword)
                {
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp");
                    return View(model);
                }

                var userId = HttpContext.Session.GetString("UserId");
                var token = HttpContext.Session.GetString("JwtToken");

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var changePasswordDto = new
                {
                    MaNguoiDung = int.Parse(userId),
                    OldPassword = model.CurrentPassword,
                    NewPassword = model.NewPassword
                };

                var jsonContent = JsonConvert.SerializeObject(changePasswordDto);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Đổi mật khẩu cho user ID: {UserId}", userId);

                var response = await client.PostAsync($"{_apiBaseUrl}/api/Account/change-password", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Đổi mật khẩu thành công user ID: {UserId}", userId);
                    TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Đổi mật khẩu thất bại. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    ModelState.AddModelError("", "Mật khẩu hiện tại không đúng hoặc có lỗi xảy ra");
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đổi mật khẩu");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi đổi mật khẩu");
                return View(model);
            }
        }
    }
}
