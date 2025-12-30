using Microsoft.AspNetCore.Mvc;
using Petcare_web.Models.DTO;
using System.Net.Http;
namespace Petcare_web.Controllers.Staff
{
    public class StaffPetsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? _apiBaseUrl;

        public StaffPetsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }
        // [GET] /StaffPets/Index
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

        // [GET] /StaffPets/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Chi tiết thú cưng";
            
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                
                // Get pet details
                var petResponse = await client.GetAsync($"{_apiBaseUrl}/api/ThuCung/addthucungById/{id}");
                
                if (!petResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thú cưng.";
                    return RedirectToAction("Index");
                }

                var pet = await petResponse.Content.ReadFromJsonAsync<ThuCungDTO>();
                
                if (pet == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy thú cưng.";
                    return RedirectToAction("Index");
                }

                // Get customer details
                KhachHangDTO? customer = null;
                try
                {
                    var customerResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/getkhachhangById/{pet.MaKhachHang}");
                    if (customerResponse.IsSuccessStatusCode)
                    {
                        customer = await customerResponse.Content.ReadFromJsonAsync<KhachHangDTO>();
                    }
                }
                catch (Exception ex)
                {
                    // Continue without customer info if it fails
                    ViewData["CustomerError"] = $"Không thể tải thông tin khách hàng: {ex.Message}";
                }

                // Create a view model
                var viewModel = new
                {
                    Pet = pet,
                    Customer = customer
                };

                ViewBag.Pet = pet;
                ViewBag.Customer = customer;

                return View(pet);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Đã xảy ra lỗi: {ex.Message}";
                return RedirectToAction("Index");
            }
        }
    }
}
