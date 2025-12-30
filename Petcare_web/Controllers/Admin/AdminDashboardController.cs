using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using Petcare_web.Models.DTO;

namespace Petcare_web.Controllers.Admin
{
    [Authorize(Roles = "QUANTRI")]
    [Route("Admin/Dashboard")]
    public class AdminDashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;

        public AdminDashboardController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7053";
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";

            var token = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                Console.WriteLine("=== ADMIN DASHBOARD DEBUG ===");
                Console.WriteLine($"API Base URL: {_apiBaseUrl}");
                Console.WriteLine($"Token: {token?.Substring(0, Math.Min(20, token.Length))}...");

                // 1. Lấy tổng số khách hàng (tất cả)
                var khachHangResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/getallkhachhang?pageSize=1000");
                Console.WriteLine($"KhachHang API Status: {khachHangResponse.StatusCode}");
                if (khachHangResponse.IsSuccessStatusCode)
                {
                    var khData = await khachHangResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"KhachHang Response Length: {khData.Length} characters");
                    Console.WriteLine($"KhachHang Response Preview: {khData.Substring(0, Math.Min(300, khData.Length))}...");
                    var khachHangList = JsonConvert.DeserializeObject<List<dynamic>>(khData);
                    ViewBag.TotalCustomers = khachHangList?.Count ?? 0;
                    Console.WriteLine($"✅ Total Customers: {ViewBag.TotalCustomers}");
                }
                else
                {
                    Console.WriteLine($"❌ KhachHang API failed: {khachHangResponse.StatusCode}");
                    ViewBag.TotalCustomers = 0;
                }

                // 2. Lấy tổng số nhân viên (tất cả)
                var nhanVienResponse = await client.GetAsync($"{_apiBaseUrl}/api/NhanVien/getallnhanvien?pageSize=1000");
                Console.WriteLine($"NhanVien API Status: {nhanVienResponse.StatusCode}");
                if (nhanVienResponse.IsSuccessStatusCode)
                {
                    var nvData = await nhanVienResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"NhanVien Response Length: {nvData.Length} characters");
                    Console.WriteLine($"NhanVien Response Preview: {nvData.Substring(0, Math.Min(300, nvData.Length))}...");
                    var nhanVienList = JsonConvert.DeserializeObject<List<dynamic>>(nvData);
                    ViewBag.TotalStaff = nhanVienList?.Count ?? 0;
                    ViewBag.ActiveStaff = nhanVienList?.Count(nv => (bool)nv.trangThai) ?? 0;
                    Console.WriteLine($"✅ Total Staff: {ViewBag.TotalStaff}, Active: {ViewBag.ActiveStaff}");
                }
                else
                {
                    Console.WriteLine($"❌ NhanVien API failed: {nhanVienResponse.StatusCode}");
                    ViewBag.TotalStaff = 0;
                    ViewBag.ActiveStaff = 0;
                }

                // 3. Lấy tổng số lịch hẹn (tất cả)
                var lichHenResponse = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/alllichhen?pageSize=10000");
                Console.WriteLine($"LichHen API Status: {lichHenResponse.StatusCode}");
                List<LichHenDTO> allAppointments = new List<LichHenDTO>();
                if (lichHenResponse.IsSuccessStatusCode)
                {
                    var lhData = await lichHenResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"LichHen Response Length: {lhData.Length} characters");
                    Console.WriteLine($"LichHen Response Preview: {lhData.Substring(0, Math.Min(300, lhData.Length))}...");
                    allAppointments = JsonConvert.DeserializeObject<List<LichHenDTO>>(lhData) ?? new List<LichHenDTO>();
                    
                    // Tổng tất cả lịch hẹn
                    ViewBag.TotalAppointments = allAppointments.Count;
                    Console.WriteLine($"✅ Total Appointments: {ViewBag.TotalAppointments}");
                    
                    // 5 lịch hẹn gần nhất
                    ViewBag.RecentAppointments = allAppointments
                        .OrderByDescending(a => a.NgayGioHen)
                        .Take(5)
                        .ToList();
                }
                else
                {
                    Console.WriteLine($"❌ LichHen API failed: {lichHenResponse.StatusCode}");
                    ViewBag.TotalAppointments = 0;
                    ViewBag.RecentAppointments = new List<LichHenDTO>();
                }

                // 4. Tính tổng doanh thu (tất cả)
                var thanhToanResponse = await client.GetAsync($"{_apiBaseUrl}/api/ThanhToan/allthanhtoan?pageSize=10000");
                Console.WriteLine($"ThanhToan API Status: {thanhToanResponse.StatusCode}");
                if (thanhToanResponse.IsSuccessStatusCode)
                {
                    var ttData = await thanhToanResponse.Content.ReadAsStringAsync();
                    Console.WriteLine($"ThanhToan Response Length: {ttData.Length} characters");
                    Console.WriteLine($"ThanhToan Response Preview: {ttData.Substring(0, Math.Min(300, ttData.Length))}...");
                    var payments = JsonConvert.DeserializeObject<List<dynamic>>(ttData);
                    
                    decimal totalRevenue = 0;
                    foreach (var payment in payments ?? new List<dynamic>())
                    {
                        totalRevenue += (decimal)payment.soTien;
                    }
                    
                    ViewBag.TotalRevenue = totalRevenue;
                    Console.WriteLine($"✅ Total Revenue: {totalRevenue:C}");
                }
                else
                {
                    Console.WriteLine($"❌ ThanhToan API failed: {thanhToanResponse.StatusCode}");
                    ViewBag.TotalRevenue = 0;
                }

                Console.WriteLine("=== DASHBOARD LOADED SUCCESSFULLY ===");
                return View();
            }
            catch (Exception ex)
            {
                Console.WriteLine("=== ERROR IN ADMIN DASHBOARD ===");
                Console.WriteLine($"❌ Error Message: {ex.Message}");
                Console.WriteLine($"❌ Stack Trace: {ex.StackTrace}");
                Console.WriteLine($"❌ Inner Exception: {ex.InnerException?.Message}");
                
                ViewBag.ErrorMessage = $"Có lỗi khi tải dữ liệu: {ex.Message}";
                ViewBag.TotalCustomers = 0;
                ViewBag.TotalStaff = 0;
                ViewBag.ActiveStaff = 0;
                ViewBag.TotalAppointments = 0;
                ViewBag.TotalRevenue = 0;
                ViewBag.RecentAppointments = new List<LichHenDTO>();
                return View();
            }
        }
    }
}