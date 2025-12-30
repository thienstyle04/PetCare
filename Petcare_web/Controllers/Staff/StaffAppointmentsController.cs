using Microsoft.AspNetCore.Mvc;
using Petcare_web.Models.DTO;
using System.Net.Http;
namespace Petcare_web.Controllers.Staff
{
        [Route("Staff/Appointments")]
        public class StaffAppointmentsController : Controller
        {
            private readonly IHttpClientFactory _httpClientFactory;
            private readonly string _apiBaseUrl;

            public StaffAppointmentsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
            {
                _httpClientFactory = httpClientFactory;
                _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
            }

        // [GET] /Staff/Appointments/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý Lịch hẹn";
            var appointments = new List<LichHenDTO>();
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                // ✅ ĐIỀU CHỈNH: Khớp với endpoint [HttpGet("alllichhen")]
                var response = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/alllichhen");
                response.EnsureSuccessStatusCode();
                appointments = await response.Content.ReadFromJsonAsync<List<LichHenDTO>>() ?? new List<LichHenDTO>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải danh sách lịch hẹn: {ex.Message}";
            }
            return View(appointments);
        }

        // [POST] /Staff/Appointments/UpdateStatus
        [HttpPost("UpdateStatus")]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var statusDto = new UpdateStatusDTO { TrangThai = status };
                
                var response = await client.PatchAsJsonAsync($"{_apiBaseUrl}/api/LichHen/{id}/status", statusDto);
                
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = $"Cập nhật trạng thái thành công: {status}";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"Lỗi khi cập nhật trạng thái: {error}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
}
