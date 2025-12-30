using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using PetCare_web.Models.DTO;
using Petcare_web.Models.DTO;
using System.Net.Http.Headers;

namespace Petcare_web.Controllers.Staff
{
    [Authorize(Roles = "NHANVIEN")]
    [Route("Staff/Dashboard")]
    public class StaffDashboardController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public StaffDashboardController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7053";
        }

        [Route("")]
        [Route("Index")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Dashboard";

            var token = HttpContext.Session.GetString("JwtToken");
            var userId = HttpContext.Session.GetString("UserId");

            if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 1. Lấy ID nhân viên từ UserId
                var nhanVienIdResponse = await client.GetAsync($"{_apiBaseUrl}/api/NhanVien/getEmployeeIdByUserId/{userId}");
                if (!nhanVienIdResponse.IsSuccessStatusCode)
                    return RedirectToAction("Login", "Account");

                var nhanVienIdData = await nhanVienIdResponse.Content.ReadAsStringAsync();
                var maNhanVien = JsonConvert.DeserializeObject<int>(nhanVienIdData);

                if (maNhanVien == 0)
                    return RedirectToAction("Login", "Account");

                // 2. Lấy danh sách lịch hẹn của nhân viên
                var appointmentsResponse = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/nhanvien/{maNhanVien}");
                
                List<LichHenDTO> appointments = new List<LichHenDTO>();
                if (appointmentsResponse.IsSuccessStatusCode)
                {
                    var appointmentsData = await appointmentsResponse.Content.ReadAsStringAsync();
                    appointments = JsonConvert.DeserializeObject<List<LichHenDTO>>(appointmentsData) ?? new List<LichHenDTO>();
                }

                // 3. Tính toán thống kê
                var today = DateTime.Today;
                var todayAppointments = appointments.Where(a => a.NgayGioHen.Date == today).ToList();

                ViewBag.TodayTotal = todayAppointments.Count;
                ViewBag.CompletedToday = todayAppointments.Count(a => a.TrangThai == "Hoàn thành" || a.TrangThai == "Đã hoàn thành");
                ViewBag.PendingToday = todayAppointments.Count(a => a.TrangThai == "Chờ xác nhận" || a.TrangThai == "Đã xác nhận");
                
                // Lấy số hồ sơ dịch vụ đã tạo bởi nhân viên này
                var hoSoResponse = await client.GetAsync($"{_apiBaseUrl}/api/HoSoDichVu/allhosodichvu?pageSize=1000");
                if (hoSoResponse.IsSuccessStatusCode)
                {
                    var hoSoData = await hoSoResponse.Content.ReadAsStringAsync();
                    var hoSoList = JsonConvert.DeserializeObject<List<HoSoDichVuDTO>>(hoSoData);
                    // Filter by MaNhanVien
                    ViewBag.TotalRecords = hoSoList?.Count(h => h.MaNhanVien == maNhanVien) ?? 0;
                }
                else
                {
                    ViewBag.TotalRecords = 0;
                }

                // 4. Lấy lịch hẹn hôm nay
                ViewBag.TodayAppointments = todayAppointments.OrderBy(a => a.NgayGioHen).ToList();

                return View();
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = $"Có lỗi khi tải dữ liệu: {ex.Message}";
                return View();
            }
        }
    }
}
