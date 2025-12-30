using Microsoft.AspNetCore.Mvc;
using Petcare_web.Models.DTO;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.Net.Http;

namespace Petcare_web.Controllers.Admin
{
    [Route("Admin/Customers")]
    public class AdminCustomersController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public AdminCustomersController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // [GET] /Admin/Customers/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý khách hàng";
            var customers = new List<KhachHangDTO>();

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var httpResponseMessage = await client.GetAsync($"{_apiBaseUrl}/api/KhachHang/getallkhachhang");
                httpResponseMessage.EnsureSuccessStatusCode();

                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();

                customers = JsonSerializer.Deserialize<List<KhachHangDTO>>(responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<KhachHangDTO>();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Không thể tải danh sách khách hàng: {ex.Message}";
            }

            return View(customers);
        }

        // [GET] /Admin/Customers/Details/{id}
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Chi tiết khách hàng";

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                
                // Get customer details
                var customerResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/getkhachhangById/{id}");
                
                if (!customerResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                    return RedirectToAction("Index");
                }

                var responseBody = await customerResponse.Content.ReadAsStringAsync();
                var customer = JsonSerializer.Deserialize<KhachHangDTO>(responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (customer == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy khách hàng.";
                    return RedirectToAction("Index");
                }

                // Get customer's pets
                List<ThuCungDTO>? pets = null;
                try
                {
                    var petsResponse = await client.GetAsync($"{_apiBaseUrl}/api/ThuCung/khachhang/{id}");
                    if (petsResponse.IsSuccessStatusCode)
                    {
                        var petsBody = await petsResponse.Content.ReadAsStringAsync();
                        pets = JsonSerializer.Deserialize<List<ThuCungDTO>>(petsBody,
                            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    }
                }
                catch (Exception ex)
                {
                    ViewData["PetsError"] = $"Không thể tải danh sách thú cưng: {ex.Message}";
                }

                ViewBag.Pets = pets;
                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // [GET] /Admin/Customers/Create
        [HttpGet]
        [Route("Create")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Thêm mới khách hàng";
            return View();
        }
        [HttpPost]
        [Route("Create")]
        public async Task<IActionResult> Create(RegisterKhachHangDTO addCustomerRequest)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(addCustomerRequest),
                    Encoding.UTF8,
                    "application/json");

                // ⚙️ Gọi API mới trong backend
                var httpResponseMessage = await client.PostAsync(
                    $"{_apiBaseUrl}/api/Khachhang/addkhachhang", jsonContent);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm khách hàng thành công!";
                    return RedirectToAction("Index");
                }

                var errorResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                ViewData["ErrorMessage"] = $"Có lỗi xảy ra khi thêm khách hàng: {errorResponse}";
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Lỗi khi gọi API: {ex.Message}";
            }

            return View(addCustomerRequest);
        }

        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Chỉnh sửa khách hàng";

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var httpResponseMessage = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/getkhachhangById/{id}");
                httpResponseMessage.EnsureSuccessStatusCode();

                var responseBody = await httpResponseMessage.Content.ReadAsStringAsync();
                var customer = JsonSerializer.Deserialize<KhachHangDTO>(responseBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (customer == null)
                    return NotFound();

                return View(customer);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải thông tin khách hàng: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // [POST] /Admin/Customers/Edit/{id}
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, KhachHangDTO updateCustomerRequest)
        {
            ViewData["Title"] = "Chỉnh sửa khách hàng";

            if (!ModelState.IsValid)
            {
                ViewData["ErrorMessage"] = "Dữ liệu không hợp lệ.";
                return View(updateCustomerRequest);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var jsonContent = new StringContent(
                    JsonSerializer.Serialize(updateCustomerRequest),
                    Encoding.UTF8,
                    "application/json"
                );

                // 🔥 Lưu ý: API backend là UpdateKhachHang/{maKhachHang:int}
                var httpResponseMessage = await client.PutAsync($"{_apiBaseUrl}/api/Khachhang/UpdateKhachHang/{id}", jsonContent);
                httpResponseMessage.EnsureSuccessStatusCode();

                TempData["SuccessMessage"] = "Cập nhật khách hàng thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Có lỗi khi cập nhật: {ex.Message}";
                return View(updateCustomerRequest);
            }
        }

        // [GET] /Admin/Customers/Delete/{id}
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var client = _httpClientFactory.CreateClient("PetCareApiClient");

            try
            {
                // Gọi API xóa khách hàng
                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/Khachhang/DeleteKhachHangById/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "🗑️ Xóa khách hàng thành công!";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    TempData["ErrorMessage"] = "❌ Không tìm thấy khách hàng để xóa.";
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"⚠️ Lỗi khi xóa khách hàng: {errorMsg}";
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["ErrorMessage"] = $"🚫 Lỗi kết nối đến API: {ex.Message}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"⚠️ Đã xảy ra lỗi khi xóa khách hàng: {ex.Message}";
            }

            // Quay về danh sách khách hàng
            return RedirectToAction("Index");
        }
    }
}
