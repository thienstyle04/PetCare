using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Petcare_web.Models.DTO;
using System.Net.Http.Headers;
using System.Text;

namespace Petcare_web.Controllers
{
    [Authorize(Roles = "KHACHHANG")]
    public class CustomerPetsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public CustomerPetsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7179";
        }

        // 🐕 Danh sách thú cưng
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // 1. Lấy MaKhachHang từ MaNguoiDung
                var khResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{userId}");
                
                if (!khResponse.IsSuccessStatusCode)
                {
                    return View("Index", new List<ThuCungDTO>());
                }

                var khData = await khResponse.Content.ReadAsStringAsync();
                var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(khData);
                
                if (khachHang == null)
                {
                    return View("Index", new List<ThuCungDTO>());
                }

                // 2. Lấy danh sách thú cưng theo MaKhachHang
                var response = await client.GetAsync($"{_apiBaseUrl}/api/ThuCung/khachhang/{khachHang.MaKhachHang}");
                var pets = new List<ThuCungDTO>();

                if (response.IsSuccessStatusCode)
                {
                    pets = JsonConvert.DeserializeObject<List<ThuCungDTO>>(
                        await response.Content.ReadAsStringAsync()) ?? new List<ThuCungDTO>();
                }

                return View("Index", pets);
            }
            catch (Exception)
            {
                return View("Index", new List<ThuCungDTO>());
            }
        }

        // 🐶 Form thêm thú cưng
        [HttpGet]
        public IActionResult Add() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ThuCungDTO petDto)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(petDto);

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 1. Lấy MaKhachHang từ MaNguoiDung
                var khResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{userId}");
                
                if (!khResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Không thể xác định thông tin khách hàng.");
                    return View(petDto);
                }

                var khData = await khResponse.Content.ReadAsStringAsync();
                var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(khData);
                
                if (khachHang == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin khách hàng.");
                    return View(petDto);
                }

                // 2. Gán MaKhachHang vào DTO
                petDto.MaKhachHang = khachHang.MaKhachHang;

                // 3. Gửi request thêm thú cưng
                var json = JsonConvert.SerializeObject(petDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBaseUrl}/api/ThuCung/addthucung", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Không thể thêm thú cưng. Chi tiết: {errorContent}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            }

            return View(petDto);
        }

        // 📝 Edit Pet
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_apiBaseUrl}/api/ThuCung/addthucungById/{id}");
            
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await response.Content.ReadAsStringAsync();
            var pet = JsonConvert.DeserializeObject<ThuCungDTO>(json);

            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ThuCungDTO petDto)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid)
                return View(petDto);

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var json = JsonConvert.SerializeObject(petDto);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PutAsync($"{_apiBaseUrl}/api/ThuCung/updatepet/{id}", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                var errorContent = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError("", $"Không thể cập nhật thú cưng. Chi tiết: {errorContent}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            }

            return View(petDto);
        }

        // �️ Detail Pet
        [HttpGet]
        public async Task<IActionResult> Detail(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_apiBaseUrl}/api/ThuCung/addthucungById/{id}");
            
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await response.Content.ReadAsStringAsync();
            var pet = JsonConvert.DeserializeObject<ThuCungDTO>(json);

            return View(pet);
        }

        // �🗑️ Delete Pet
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"{_apiBaseUrl}/api/ThuCung/addthucungById/{id}");
            
            if (!response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            var json = await response.Content.ReadAsStringAsync();
            var pet = JsonConvert.DeserializeObject<ThuCungDTO>(json);

            return View(pet);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.DeleteAsync($"{_apiBaseUrl}/api/ThuCung/DeleteByID/{id}");

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Xóa thú cưng thành công!";
                    return RedirectToAction("Index");
                }

                TempData["ErrorMessage"] = "Không thể xóa thú cưng.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
