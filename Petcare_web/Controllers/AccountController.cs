using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Petcare_web.Models.DTO;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Petcare_web.Controllers
{
    public class AccountController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public AccountController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            // Trả về view cho form đăng ký
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterNguoiDungDTO model)
        {
            if (!ModelState.IsValid)
            {
                // Nếu dữ liệu form không hợp lệ, trả về view với lỗi
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var json = JsonSerializer.Serialize(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Gọi API để thực hiện đăng ký
                var response = await client.PostAsync($"{_apiBaseUrl}/api/Account/Register", content);

                if (response.IsSuccessStatusCode)
                {
                    // Đăng ký thành công, hiển thị thông báo và chuyển hướng đến trang đăng nhập
                    TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
                    return RedirectToAction("Login", "Account");
                }
                else
                {
                    // Xử lý lỗi từ API (ví dụ: email đã tồn tại)
                    var errorContent = await response.Content.ReadAsStringAsync();
                    ModelState.AddModelError(string.Empty, $"Đăng ký thất bại: {errorContent}");
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi kết nối hoặc lỗi hệ thống
                ModelState.AddModelError(string.Empty, "Đã có lỗi xảy ra. Vui lòng thử lại sau.");
            }

            // Trả về view với thông tin đã nhập nếu có lỗi
            return View(model);
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginNguoiDungDTO model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                var jsonPayload = JsonSerializer.Serialize(model);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await client.PostAsync($"{_apiBaseUrl}/api/Account/Login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var loginResponse = JsonSerializer.Deserialize<NguoiDungResponseDTO>(responseBody,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.JwtToken) && loginResponse.Roles.Any())
                    {
                        // === DEBUG: Log roles ===
                        Console.WriteLine($"DEBUG Login - User: {model.TenDangNhap}");
                        Console.WriteLine($"DEBUG Login - Roles from API: {string.Join(", ", loginResponse.Roles)}");
                        
                        // === TẠO CLAIMS ===
                        var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, model.TenDangNhap),
                    new Claim("MaNguoiDung", loginResponse.MaNguoiDung.ToString())
                };

                        foreach (var role in loginResponse.Roles)
                        {
                            claims.Add(new Claim(ClaimTypes.Role, role.ToUpper()));
                        }

                        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        // Đăng nhập (cookie-based)
                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(claimsIdentity));

                        // === LƯU DỮ LIỆU VÀO SESSION ===
                        HttpContext.Session.SetString("UserId", loginResponse.MaNguoiDung.ToString());
                        HttpContext.Session.SetString("JwtToken", loginResponse.JwtToken);
                        HttpContext.Session.SetString("Roles", string.Join(",", loginResponse.Roles));

                        // === ĐIỀU HƯỚNG THEO VAI TRÒ ===
                        // Ưu tiên role cao nhất: QUANTRI > NHANVIEN > KHACHHANG
                        string userRole;
                        if (loginResponse.Roles.Any(r => r.Equals("QUANTRI", StringComparison.OrdinalIgnoreCase)))
                        {
                            userRole = "QUANTRI";
                        }
                        else if (loginResponse.Roles.Any(r => r.Equals("NHANVIEN", StringComparison.OrdinalIgnoreCase)))
                        {
                            userRole = "NHANVIEN";
                        }
                        else
                        {
                            userRole = loginResponse.Roles.First().ToUpper();
                        }
                        
                        Console.WriteLine($"DEBUG Login - Selected role for redirect: {userRole}");
                        
                        switch (userRole)
                        {
                            case "QUANTRI":
                                return RedirectToAction("Index", "AdminDashboard");
                            case "NHANVIEN":
                                return RedirectToAction("Index", "StaffDashboard");
                            case "KHACHHANG":
                                return RedirectToAction("Index", "Khachhang");
                            default:
                                return RedirectToAction("Index", "Home");
                        }
                    }
                }

                ModelState.AddModelError(string.Empty, "Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError(string.Empty, "Không thể kết nối đến máy chủ. Vui lòng thử lại sau.");
            }

            return View(model);
        }
        [AllowAnonymous]
        [HttpGet]
        public IActionResult Create()
        {
            Console.WriteLine(">>> Đã vào Customer/Create <<<");
            return View();
        }
        // GET: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            Response.Cookies.Delete("JwtToken"); // Xóa token
            return RedirectToAction("Login", "Account");
        }

        
    }
}