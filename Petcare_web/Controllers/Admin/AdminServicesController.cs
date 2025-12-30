using Microsoft.AspNetCore.Mvc;
using Petcare_web.Models.DTO;
using System.Net.Http.Json;
using System.Text.Json;
using System.Security.Claims;
using System.Net.Http;

namespace Petcare_web.Controllers.Admin
{
    [Route("Admin/Services")]
    public class AdminServicesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public AdminServicesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // [GET] /Admin/Services/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index(int pageNumber = 1, int pageSize = 10)
        {
            ViewData["Title"] = "Quản lý Dịch vụ";
            var services = new List<DichVuDTO>();
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                
                // Lấy tổng số dịch vụ (không phân trang)
                var allResponse = await client.GetAsync($"{_apiBaseUrl}/api/DichVu/alldichvu?pageSize=1000");
                var allServices = await allResponse.Content.ReadFromJsonAsync<List<DichVuDTO>>() ?? new List<DichVuDTO>();
                int totalItems = allServices.Count;
                
                // Lấy dữ liệu theo trang
                var response = await client.GetAsync($"{_apiBaseUrl}/api/DichVu/alldichvu?pageNumber={pageNumber}&pageSize={pageSize}");
                response.EnsureSuccessStatusCode();
                services = await response.Content.ReadFromJsonAsync<List<DichVuDTO>>() ?? new List<DichVuDTO>();
                
                // Truyền thông tin phân trang sang View
                ViewBag.CurrentPage = pageNumber;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = totalItems;
                ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải danh sách dịch vụ: {ex.Message}";
                ViewBag.CurrentPage = 1;
                ViewBag.PageSize = pageSize;
                ViewBag.TotalItems = 0;
                ViewBag.TotalPages = 0;
            }
            return View(services);
        }

        // [GET] /Admin/Services/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm mới Dịch vụ";
            return View();
        }

        // [POST] /Admin/Services/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(DichVuDTO createRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(createRequest);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                // ✅ ĐIỀU CHỈNH: Khớp với endpoint [HttpPost("themdichvu")]
                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/DichVu/themdichvu", createRequest);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm dịch vụ thành công!";
                    return RedirectToAction("Index");
                }
                var error = await response.Content.ReadAsStringAsync();
                ViewData["ErrorMessage"] = $"Lỗi khi thêm dịch vụ: {error}";
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
            }
            return View(createRequest);
        }

        // [GET] /Admin/Services/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Chỉnh sửa Dịch vụ";
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                // ✅ ĐIỀU CHỈNH: Khớp với endpoint [HttpGet("Get_dichVu_ID/{MaDichVu:int}")]
                var service = await client.GetFromJsonAsync<DichVuDTO>($"{_apiBaseUrl}/api/DichVu/Get_dichVu_ID/{id}");
                if (service == null)
                {
                    return NotFound();
                }
                return View(service);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải thông tin dịch vụ: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // [POST] /Admin/Services/Edit/{id}
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, DichVuDTO updateRequest)
        {
            if (!ModelState.IsValid)
            {
                return View(updateRequest);
            }
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                // ✅ ĐIỀU CHỈNH: Khớp với endpoint [HttpPut("update_dichvu_ID/{MaDichVu:int}")]
                var response = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/DichVu/update_dichvu_ID/{id}", updateRequest);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật dịch vụ thành công!";
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

        // [GET] /Admin/Services/Delete/{id}
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                // ✅ ĐIỀU CHỈNH: Khớp với endpoint [HttpDelete("delete_dichvu_id/{MaDichVu:int}")]
                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/DichVu/delete_dichvu_id/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "🗑️ Xóa dịch vụ thành công!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"⚠️ Lỗi khi xóa dịch vụ: {error}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"🚫 Lỗi hệ thống: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
    }
}