using Microsoft.AspNetCore.Mvc;
using PetCare_web.Models.DTO;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.Net.Http;

namespace PetCare.Controllers.Admin
{
    [Route("Admin/Staff")]
    public class AdminStaffController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AdminStaffController> _logger;
        private readonly string _apiBaseUrl;

        public AdminStaffController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<AdminStaffController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // [GET] /Admin/Staff/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index(
            string? filteron,
            string? filterQuery,
            string? sortBy,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 10)
        {
            ViewData["Title"] = "Quản lý nhân viên";
            var employees = new List<NhanVienDTO>();

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");

                // Build query string
                var queryParams = new List<string>();
                if (!string.IsNullOrEmpty(filteron)) queryParams.Add($"filteron={filteron}");
                if (!string.IsNullOrEmpty(filterQuery)) queryParams.Add($"filterQuery={filterQuery}");
                if (!string.IsNullOrEmpty(sortBy)) queryParams.Add($"sortBy={sortBy}");
                queryParams.Add($"isAscending={isAscending}");
                queryParams.Add($"pageNumber={pageNumber}");
                queryParams.Add($"pageSize={pageSize}");

                var queryString = string.Join("&", queryParams);
                var url = $"{_apiBaseUrl}/api/NhanVien/getallnhanvien?{queryString}";

                _logger.LogInformation("Calling API: {Url}", url);

                var httpResponseMessage = await client.GetAsync(url);
                httpResponseMessage.EnsureSuccessStatusCode();

                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();

                employees = JsonSerializer.Deserialize<List<NhanVienDTO>>(responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<NhanVienDTO>();

                _logger.LogInformation("Loaded {Count} employees", employees.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employees list");
                ViewData["ErrorMessage"] = $"Không thể tải danh sách nhân viên: {ex.Message}";
            }

            return View(employees);
        }

        // [GET] /Admin/Staff/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm mới nhân viên";
            return View();
        }

        // [POST] /Admin/Staff/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(RegisterNhanVienDTO addEmployeeRequest)
        {
            ViewData["Title"] = "Thêm mới nhân viên";

            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                return View(addEmployeeRequest);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(addEmployeeRequest),
                    Encoding.UTF8,
                    "application/json");

                _logger.LogInformation("Creating new employee: {Username}", addEmployeeRequest.TenDangNhap);

                var httpResponseMessage = await client.PostAsync(
                    $"{_apiBaseUrl}/api/NhanVien/addnhanvien", jsonContent);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Employee created successfully");
                    TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                    return RedirectToAction("Index");
                }

                var errorResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to create employee: {Error}", errorResponse);
                ViewData["ErrorMessage"] = $"Có lỗi xảy ra khi thêm nhân viên: {errorResponse}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API to create employee");
                ViewData["ErrorMessage"] = $"Lỗi khi gọi API: {ex.Message}";
            }

            return View(addEmployeeRequest);
        }

        // [GET] /Admin/Staff/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Chỉnh sửa nhân viên";

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var httpResponseMessage = await client.GetAsync($"{_apiBaseUrl}/api/NhanVien/getnhanvienById/{id}");
                httpResponseMessage.EnsureSuccessStatusCode();

                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                var employee = JsonSerializer.Deserialize<NhanVienDTO>(responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (employee == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy nhân viên";
                    return RedirectToAction("Index");
                }

                return View(employee);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading employee for edit, ID: {Id}", id);
                TempData["ErrorMessage"] = $"Không thể tải thông tin nhân viên: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // [POST] /Admin/Staff/Edit/{id}
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, NhanVienDTO updateEmployeeRequest)
        {
            ViewData["Title"] = "Chỉnh sửa nhân viên";

            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                return View(updateEmployeeRequest);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");

                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(updateEmployeeRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                _logger.LogInformation("Updating employee ID: {Id}", id);

                var httpResponseMessage = await client.PutAsync(
                    $"{_apiBaseUrl}/api/NhanVien/UpdateNhanVien/{id}", jsonContent);

                httpResponseMessage.EnsureSuccessStatusCode();

                _logger.LogInformation("Employee ID {Id} updated successfully", id);
                TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating employee ID: {Id}", id);
                ViewData["ErrorMessage"] = $"Có lỗi khi cập nhật: {ex.Message}";
                return View(updateEmployeeRequest);
            }
        }

        // [GET] /Admin/Staff/Delete/{id}
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("PetCareApiClient");

            try
            {
                _logger.LogInformation("Deleting employee ID: {Id}", id);

                // Gọi API xóa nhân viên
                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/NhanVien/DeleteNhanVienById/{id}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Employee ID {Id} deleted successfully", id);
                    TempData["SuccessMessage"] = "🗑️ Xóa nhân viên thành công!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    _logger.LogWarning("Employee ID {Id} not found for deletion", id);
                    TempData["ErrorMessage"] = "❌ Không tìm thấy nhân viên để xóa.";
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Failed to delete employee ID {Id}: {Error}", id, errorMsg);
                    TempData["ErrorMessage"] = $"⚠️ Lỗi khi xóa nhân viên: {errorMsg}";
                }
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "HTTP error deleting employee ID: {Id}", id);
                TempData["ErrorMessage"] = $"🚫 Lỗi kết nối đến API: {ex.Message}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error deleting employee ID: {Id}", id);
                TempData["ErrorMessage"] = $"⚠️ Đã xảy ra lỗi khi xóa nhân viên: {ex.Message}";
            }

            // Quay về danh sách nhân viên
            return RedirectToAction("Index");
        }
    }
}