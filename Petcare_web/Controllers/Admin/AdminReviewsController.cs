using Microsoft.AspNetCore.Mvc;
using Petcare_web.Models.DTO;
using System.Security.Claims;
using System.Net.Http;
namespace Petcare_web.Controllers.Admin
{
    [Route("Admin/Reviews")]
    public class AdminReviewsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public AdminReviewsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // [GET] /Admin/Reviews/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Quản lý Đánh giá";
            var reviews = new List<DanhGiaDTO>();
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var response = await client.GetAsync($"{_apiBaseUrl}/api/DanhGia");
                response.EnsureSuccessStatusCode();
                reviews = await response.Content.ReadFromJsonAsync<List<DanhGiaDTO>>() ?? new List<DanhGiaDTO>();
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải danh sách đánh giá: {ex.Message}";
            }
            return View(reviews);
        }

        // [GET] /Admin/Reviews/Details/{id}
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Chi tiết Đánh giá";
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var review = await client.GetFromJsonAsync<DanhGiaDTO>($"{_apiBaseUrl}/api/DanhGia/{id}");
                if (review == null)
                {
                    return NotFound();
                }
                return View(review);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải chi tiết đánh giá: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // [GET] /Admin/Reviews/Delete/{id}
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/DanhGia/{id}");
                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "🗑️ Xóa đánh giá thành công!";
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    TempData["ErrorMessage"] = $"⚠️ Lỗi khi xóa đánh giá: {error}";
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