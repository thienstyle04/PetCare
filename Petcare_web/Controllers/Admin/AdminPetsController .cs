using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Petcare_web.Models.DTO;
using System.Text;
using System.Text.Json;
using System.Security.Claims;
using System.Net.Http;
namespace Petcare_web.Controllers.Admin
{
    [Route("Admin/Pets")]
    public class AdminPetsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public AdminPetsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // [GET] /Admin/Pets/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý thú cưng";
            var pets = new List<ThuCungDTO>();

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var httpResponseMessage = await client.GetAsync($"{_apiBaseUrl}/api/ThuCung/getallthucung");
                httpResponseMessage.EnsureSuccessStatusCode();
                pets = await httpResponseMessage.Content.ReadFromJsonAsync<List<ThuCungDTO>>() ?? new List<ThuCungDTO>();
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Không thể tải danh sách thú cưng: {ex.Message}";
            }

            return View(pets);
        }

        // [GET] /Admin/Pets/Details/{id}
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Chi tiết thú cưng";
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var pet = await client.GetFromJsonAsync<ThuCungDTO>($"{_apiBaseUrl}/api/ThuCung/addthucungById/{id}");

                if (pet == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thú cưng";
                    return RedirectToAction("Index");
                }

                return View(pet);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải thông tin thú cưng: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // [GET] /Admin/Pets/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Thêm mới thú cưng";
            await LoadCustomersIntoViewBag();
            return View();
        }

        // [POST] /Admin/Pets/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(ThuCungDTO addPetRequest)
        {
            if (!ModelState.IsValid)
            {
                await LoadCustomersIntoViewBag();
                return View(addPetRequest);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var httpResponseMessage = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/ThuCung/addthucung", addPetRequest);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thêm thú cưng thành công!";
                    return RedirectToAction("Index");
                }
                var errorResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                ViewData["ErrorMessage"] = $"Có lỗi xảy ra khi thêm thú cưng: {errorResponse}";
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Lỗi khi gọi API: {ex.Message}";
            }

            await LoadCustomersIntoViewBag();
            return View(addPetRequest);
        }

        // [GET] /Admin/Pets/Edit/{id}
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            ViewData["Title"] = "Chỉnh sửa thú cưng";
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var pet = await client.GetFromJsonAsync<ThuCungDTO>($"{_apiBaseUrl}/api/ThuCung/addthucungById/{id}");

                if (pet == null) return NotFound();

                await LoadCustomersIntoViewBag(pet.MaKhachHang);
                return View(pet);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải thông tin thú cưng: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // [POST] /Admin/Pets/Edit/{id}
        [HttpPost("Edit/{id}")]
        public async Task<IActionResult> Edit(int id, ThuCungDTO updatePetRequest)
        {
            if (!ModelState.IsValid)
            {
                await LoadCustomersIntoViewBag(updatePetRequest.MaKhachHang);
                return View(updatePetRequest);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var httpResponseMessage = await client.PutAsJsonAsync($"{_apiBaseUrl}/api/ThuCung/updatepet/{id}", updatePetRequest);

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Cập nhật thú cưng thành công!";
                    return RedirectToAction("Index");
                }
                var errorResponse = await httpResponseMessage.Content.ReadAsStringAsync();
                ViewData["ErrorMessage"] = $"Có lỗi khi cập nhật: {errorResponse}";
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Lỗi khi cập nhật API: {ex.Message}";
            }

            await LoadCustomersIntoViewBag(updatePetRequest.MaKhachHang);
            return View(updatePetRequest);
        }

        // [GET] /Admin/Pets/Delete/{id}
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/ThuCung/DeleteByID/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "🗑️ Xóa thú cưng thành công!";
                }
                else
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"⚠️ Lỗi khi xóa thú cưng: {errorMsg}";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"🚫 Đã xảy ra lỗi: {ex.Message}";
            }
            return RedirectToAction("Index");
        }

        // Helper method to load customers
        private async Task LoadCustomersIntoViewBag(object? selectedCustomer = null)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var customers = await client.GetFromJsonAsync<List<KhachHangDTO>>($"{_apiBaseUrl}/api/KhachHang/getallkhachhang");
                ViewBag.Customers = new SelectList(customers, "MaKhachHang", "HoTen", selectedCustomer);
            }
            catch (Exception)
            {
                ViewBag.Customers = new SelectList(new List<KhachHangDTO>(), "MaKhachHang", "HoTen");
                ViewData["ErrorMessage"] += " (Không thể tải danh sách khách hàng)";
            }
        }
    }
}