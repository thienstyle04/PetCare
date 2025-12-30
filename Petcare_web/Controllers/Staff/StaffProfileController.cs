using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PetCare_web.Models.DTO;
using System.Net.Http;
namespace PetCare.Controllers.Staff
{
    [Route("Staff/Profile")]
    [Authorize(Roles = "NHANVIEN")]
    public class StaffProfileController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<StaffProfileController> _logger;
        private readonly string _apiBaseUrl;

        public StaffProfileController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<StaffProfileController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // [GET] /Staff/Profile/Index/{id}
        [HttpGet("Index")]
        [HttpGet("")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Hồ sơ nhân viên";
            NhanVienDTO? staffProfile = null;

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");

                // ✅ Lấy MaNguoiDung từ claim
                var userIdClaim = User.FindFirst("MaNguoiDung")?.Value;
                if (string.IsNullOrEmpty(userIdClaim))
                {
                    ViewData["ErrorMessage"] = "Không tìm thấy MaNguoiDung trong token.";
                    return View(null);
                }

                int maNguoiDung = int.Parse(userIdClaim);

                // 1. Gọi API để đổi MaNguoiDung -> MaNhanVien
                var employeeIdResponse = await client.GetAsync($"{_apiBaseUrl}/api/NhanVien/getEmployeeIdByUserId/{maNguoiDung}");
                employeeIdResponse.EnsureSuccessStatusCode();

                var maNhanVien = await employeeIdResponse.Content.ReadFromJsonAsync<int>();

                // 2. Gọi API chính bằng MaNhanVien để lấy thông tin nhân viên
                var httpResponseMessage = await client.GetAsync($"{_apiBaseUrl}/api/NhanVien/getnhanvienById/{maNhanVien}");
                httpResponseMessage.EnsureSuccessStatusCode();
                var json = await httpResponseMessage.Content.ReadAsStringAsync();
                _logger.LogInformation("JSON trả về từ API getnhanvienById({MaNhanVien}): {Json}", maNhanVien, json);

                try
                {
                    staffProfile = System.Text.Json.JsonSerializer.Deserialize<NhanVienDTO>(json,
                        new System.Text.Json.JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (staffProfile == null)
                    {
                        _logger.LogWarning("Deserialize thành công nhưng staffProfile == null");
                    }
                    else
                    {
                        _logger.LogInformation("Deserialize thành công: MaNhanVien={MaNhanVien}, HoTen={HoTen}, Email={Email}",
                            staffProfile.MaNhanVien,
                            staffProfile.HoTen,
                            staffProfile.Email);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Deserialize lỗi với JSON: {Json}", json);
                }
                staffProfile = await httpResponseMessage.Content.ReadFromJsonAsync<NhanVienDTO>(
                    new System.Text.Json.JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                // 3. Lấy thống kê công việc của nhân viên
                var appointmentsResponse = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/nhanvien/{maNhanVien}");
                if (appointmentsResponse.IsSuccessStatusCode)
                {
                    var appointmentsJson = await appointmentsResponse.Content.ReadAsStringAsync();
                    var appointments = System.Text.Json.JsonSerializer.Deserialize<List<Petcare_web.Models.DTO.LichHenDTO>>(
                        appointmentsJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    ) ?? new List<Petcare_web.Models.DTO.LichHenDTO>();

                    var now = DateTime.Now;
                    var currentMonth = new DateTime(now.Year, now.Month, 1);

                    // Lịch hẹn tháng này
                    ViewBag.AppointmentsThisMonth = appointments.Count(a => a.NgayGioHen >= currentMonth);
                    
                    // Tổng số lịch hẹn đã hoàn thành
                    ViewBag.CompletedAppointments = appointments.Count(a => a.TrangThai == "Đã hoàn thành" || a.TrangThai == "Hoàn thành");
                    
                    // Số thú cưng unique đã chăm sóc
                    ViewBag.UniquePets = appointments
                        .Where(a => a.TrangThai == "Đã hoàn thành" || a.TrangThai == "Hoàn thành")
                        .Select(a => a.MaThuCung)
                        .Distinct()
                        .Count();
                }
                else
                {
                    ViewBag.AppointmentsThisMonth = 0;
                    ViewBag.CompletedAppointments = 0;
                    ViewBag.UniquePets = 0;
                }

                // 4. Lấy đánh giá trung bình (từ HoSoDichVu)
                var hoSoResponse = await client.GetAsync($"{_apiBaseUrl}/api/HoSoDichVu/nhanvien/{maNhanVien}");
                if (hoSoResponse.IsSuccessStatusCode)
                {
                    var hoSoJson = await hoSoResponse.Content.ReadAsStringAsync();
                    var hoSoList = System.Text.Json.JsonSerializer.Deserialize<List<dynamic>>(
                        hoSoJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                    );
                    
                    ViewBag.TotalRecords = hoSoList?.Count ?? 0;
                }
                else
                {
                    ViewBag.TotalRecords = 0;
                }

                // Đánh giá trung bình (tạm thời để mặc định, cần API đánh giá theo nhân viên)
                ViewBag.AverageRating = 0.0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff profile");
                ViewData["ErrorMessage"] = $"Không thể tải hồ sơ nhân viên: {ex.Message}";
            }

            return View(staffProfile);
        }


        // [GET] /Staff/Profile/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Cập nhật hồ sơ";

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var httpResponseMessage = await client.GetAsync($"{_apiBaseUrl}/api/NhanVien/getnhanvienById/{id}");
                httpResponseMessage.EnsureSuccessStatusCode();

                var staff = await httpResponseMessage.Content.ReadFromJsonAsync<NhanVienDTO>();
                return View(staff);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading staff profile for edit");
                TempData["ErrorMessage"] = $"Không thể tải hồ sơ nhân viên: {ex.Message}";
                return RedirectToAction("Index", new { id });
            }
        }

        // [POST] /Staff/Profile/Edit/{id}
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, NhanVienDTO updateStaffRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(updateStaffRequest);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");

                var httpResponseMessage = await client.PutAsJsonAsync(
                    $"{_apiBaseUrl}/api/NhanVien/UpdateNhanVien/{id}",
                    updateStaffRequest
                );

                httpResponseMessage.EnsureSuccessStatusCode();

                TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
                return RedirectToAction("Index", new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating staff profile ID: {Id}", id);
                ViewData["ErrorMessage"] = $"Có lỗi khi cập nhật: {ex.Message}";
                return View(updateStaffRequest);
            }
        }
    }
}
