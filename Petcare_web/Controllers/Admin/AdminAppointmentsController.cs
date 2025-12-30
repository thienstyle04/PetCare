using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Petcare_web.Models.DTO;
using PetCare_web.Models.DTO;
using System.Security.Claims;
using System.Text.Json;
using System.Net.Http;

namespace Petcare_web.Controllers.Admin
{
    [Route("Admin/Appointments")]
    public class AdminAppointmentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public AdminAppointmentsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // [GET] /Admin/Appointments/Index
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

        // [GET] /Admin/Appointments/Details/{id}
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Chi tiết Lịch hẹn";
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var response = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/get_lichhen_id/{id}");

                if (response.IsSuccessStatusCode)
                {
                    var appointment = await response.Content.ReadFromJsonAsync<LichHenDTO>();
                    if (appointment != null)
                    {
                        return View(appointment);
                    }
                }

                TempData["ErrorMessage"] = "Không tìm thấy lịch hẹn";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải thông tin lịch hẹn: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // [GET] /Admin/Appointments/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Tạo mới Lịch hẹn";
            await LoadRelatedDataIntoViewBag();
            // Sử dụng LichHenRequestDTO cho form để validation
            return View(new LichHenRequestDTO());
        }

        // [POST] /Admin/Appointments/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(LichHenRequestDTO createRequest)
        {
            if (!ModelState.IsValid)
            {
                await LoadRelatedDataIntoViewBag();
                return View(createRequest);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                // ✅ ĐIỀU CHỈNH: Khớp với endpoint [HttpPost("themlichhen")]
                // Backend nhận LichHen (domain) nên ta gửi một object có cấu trúc tương tự
                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/LichHen/themlichhen", createRequest);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Tạo lịch hẹn thành công!";
                    return RedirectToAction("Index");
                }
                var error = await response.Content.ReadAsStringAsync();
                ViewData["ErrorMessage"] = $"Lỗi khi tạo lịch hẹn: {error}";
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }

            await LoadRelatedDataIntoViewBag();
            return View(createRequest);
        }

        // [GET] /Admin/Appointments/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Chỉnh sửa Lịch hẹn";
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                // ✅ ĐIỀU CHỈNH: Khớp với endpoint [HttpGet("get_lichhen_id/{id}")]
                var appointment = await client.GetFromJsonAsync<LichHenDTO>($"{_apiBaseUrl}/api/LichHen/get_lichhen_id/{id}");

                if (appointment == null) return NotFound();

                var model = new UpdateLichHenDTO
                {
                    MaNhanVien = appointment.MaNhanVien,
                    NgayGioHen = appointment.NgayGioHen,
                    TrangThai = appointment.TrangThai,
                    GhiChu = appointment.GhiChu
                };

                await LoadRelatedDataIntoViewBag(appointment.MaKhachHang, appointment.MaThuCung, appointment.MaNhanVien);
                ViewBag.CurrentAppointment = appointment;
                return View(model);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải thông tin lịch hẹn: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // [POST] /Admin/Appointments/Edit/{id}
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, UpdateLichHenDTO updateRequest)
        {
            var client = _httpClientFactory.CreateClient("PetCareApiClient");
            // Lấy thông tin hiện tại của lịch hẹn để điền các trường bị thiếu
            var currentAppointment = await client.GetFromJsonAsync<LichHenDTO>($"{_apiBaseUrl}/api/LichHen/get_lichhen_id/{id}");

            if (currentAppointment == null)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                await LoadRelatedDataIntoViewBag(currentAppointment.MaKhachHang, currentAppointment.MaThuCung, updateRequest.MaNhanVien);
                ViewBag.CurrentAppointment = currentAppointment;
                return View(updateRequest);
            }

            try
            {
                // Tạo một đối tượng đầy đủ thông tin như LichHen domain model để gửi đi
                var payload = new
                {
                    MaLichHen = id,
                    currentAppointment.MaKhachHang,
                    currentAppointment.MaThuCung,
                    updateRequest.MaNhanVien,
                    updateRequest.NgayGioHen,
                    updateRequest.TrangThai,
                    updateRequest.GhiChu
                };

                // ✅ ĐIỀU CHỈNH: Khớp với endpoint [HttpPut("update_lichhen_id/{id}")]
                var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/LichHen/update_lichhen_id/{id}", payload);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật lịch hẹn thành công!";
                    return RedirectToAction("Index");
                }
                var error = await response.Content.ReadAsStringAsync();
                ViewData["ErrorMessage"] = $"Lỗi khi cập nhật: {error}";
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }

            await LoadRelatedDataIntoViewBag(currentAppointment.MaKhachHang, currentAppointment.MaThuCung, updateRequest.MaNhanVien);
            ViewBag.CurrentAppointment = currentAppointment;
            return View(updateRequest);
        }

        // [GET] /Admin/Appointments/Delete/{id}
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                // ✅ ĐIỀU CHỈNH: Khớp với endpoint [HttpDelete("delete_lichhen_id/{id}")]
                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/LichHen/delete_lichhen_id/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "🗑️ Xóa lịch hẹn thành công!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"⚠️ Lỗi khi xóa lịch hẹn: {error}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"🚫 Lỗi hệ thống: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // [POST] /Admin/Appointments/UpdateStatus
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

        private async Task LoadRelatedDataIntoViewBag(object? selectedCustomer = null, object? selectedPet = null, object? selectedStaff = null)
        {
            var client = _httpClientFactory.CreateClient("PetCareApiClient");
            try
            {
                var customers = await client.GetFromJsonAsync<List<KhachHangDTO>>($"{_apiBaseUrl}/api/KhachHang/getallkhachhang");
                ViewBag.Customers = new SelectList(customers, "MaKhachHang", "HoTen", selectedCustomer);

                var pets = await client.GetFromJsonAsync<List<ThuCungDTO>>($"{_apiBaseUrl}/api/ThuCung/getallthucung");
                ViewBag.Pets = new SelectList(pets, "MaThuCung", "TenThuCung", selectedPet);

                var staff = await client.GetFromJsonAsync<List<NhanVienDTO>>($"{_apiBaseUrl}/api/NhanVien/getallnhanvien");
                ViewBag.Staff = new SelectList(staff, "MaNhanVien", "HoTen", selectedStaff);

                var services = await client.GetFromJsonAsync<List<DichVuDTO>>($"{_apiBaseUrl}/api/DichVu/alldichvu");
                ViewBag.Services = new SelectList(services, "MaDichVu", "TenDichVu");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = "Không thể tải dữ liệu phụ trợ. " + ex.Message;
            }
        }
    }
}