using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Petcare_web.Models.DTO;
using System.Text.Json;
using System.Security.Claims;
using System.Net.Http;
namespace Petcare_web.Controllers.Admin
{
    [Route("Admin/Payments")]
    public class AdminPaymentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public AdminPaymentsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // [GET] /Admin/Payments/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý Thanh toán";
            var payments = new List<ThanhToanDTO>();
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var response = await client.GetAsync($"{_apiBaseUrl}/api/ThanhToan/allthanhtoan");
                response.EnsureSuccessStatusCode();
                payments = await response.Content.ReadFromJsonAsync<List<ThanhToanDTO>>() ?? new List<ThanhToanDTO>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải danh sách thanh toán: {ex.Message}";
            }
            return View(payments);
        }

        // [GET] /Admin/Payments/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Tạo mới Thanh toán";
            await LoadAppointmentsIntoViewBag();
            return View();
        }

        // [POST] /Admin/Payments/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(ThanhToanRequestDTO createRequest)
        {
            if (!ModelState.IsValid)
            {
                await LoadAppointmentsIntoViewBag();
                return View(createRequest);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/ThanhToan/themthanhtoan", createRequest);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tạo thanh toán thành công!";
                    return RedirectToAction("Index");
                }
                var error = await response.Content.ReadAsStringAsync();
                ViewData["ErrorMessage"] = $"Lỗi khi tạo thanh toán: {error}";
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }

            await LoadAppointmentsIntoViewBag();
            return View(createRequest);
        }

        // [GET] /Admin/Payments/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Cập nhật Thanh toán";
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var payment = await client.GetFromJsonAsync<ThanhToanDTO>($"{_apiBaseUrl}/api/ThanhToan/Get_thanhtoan_id/{id}");
                if (payment == null)
                {
                    return NotFound();
                }
                return View(payment);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải thông tin thanh toán: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // [POST] /Admin/Payments/Edit/{id}
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, ThanhToanDTO updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(updateRequest);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                // API backend yêu cầu một đối tượng ThanhToan đầy đủ, vì vậy chúng ta cần gửi đi cấu trúc đó
                var payload = new
                {
                    maThanhToan = id,
                    maLichHen = updateRequest.MaLichHen,
                    soTien = updateRequest.SoTien,
                    phuongThuc = updateRequest.PhuongThuc,
                    trangThai = updateRequest.TrangThai,
                    ngayThanhToan = DateTime.Now, // Cập nhật ngày thanh toán khi trạng thái thay đổi
                    ghiChu = updateRequest.GhiChu
                };

                var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/ThanhToan/update_thanhtoan_id/{id}", payload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật thanh toán thành công!";
                    return RedirectToAction("Index");
                }
                var error = await response.Content.ReadAsStringAsync();
                ViewData["ErrorMessage"] = $"Lỗi khi cập nhật: {error}";
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }
            return View(updateRequest);
        }

        // Helper method to load appointments
        private async Task LoadAppointmentsIntoViewBag(object? selectedAppointment = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var appointments = await client.GetFromJsonAsync<List<LichHenDTO>>($"{_apiBaseUrl}/api/LichHen/alllichhen");
                // Tạo text hiển thị cho dropdown
                var appointmentList = appointments?.Select(a => new {
                    MaLichHen = a.MaLichHen,
                    DisplayText = $"#{a.MaLichHen} - {a.TenKhachHang} - {a.NgayGioHen:dd/MM/yyyy HH:mm}"
                });
                ViewBag.Appointments = new SelectList(appointmentList, "MaLichHen", "DisplayText", selectedAppointment);
            }
            catch (Exception)
            {
                ViewBag.Appointments = new SelectList(new List<LichHenDTO>(), "MaLichHen", "DisplayText");
            }
        }
    }
}