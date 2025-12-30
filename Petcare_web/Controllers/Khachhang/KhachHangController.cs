using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Petcare_web.Models.DTO;
using System.Net.Http.Headers;
using System.Text;

namespace Petcare_web.Controllers
{
    [Authorize(Roles = "KHACHHANG")]
    public class KhachHangController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public KhachHangController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // === Dashboard chính của khách hàng ===
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var userId = HttpContext.Session.GetString("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // 1️⃣ Lấy hồ sơ khách hàng
            var response = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{userId}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["ErrorMessage"] = "Không thể tải hồ sơ khách hàng.";
                return RedirectToAction("Create");
            }

            var json = await response.Content.ReadAsStringAsync();
            var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(json);

            if (khachHang == null)
                return RedirectToAction("Create");

            // 2️⃣ Lấy danh sách thú cưng
            var petsResponse = await client.GetAsync($"{_apiBaseUrl}/api/ThuCung/khachhang/{khachHang.MaKhachHang}");
            var pets = petsResponse.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<ThuCungDTO>>(await petsResponse.Content.ReadAsStringAsync()) ?? new List<ThuCungDTO>()
                : new List<ThuCungDTO>();

            // 3️⃣ Lấy danh sách lịch hẹn
            var lichHenResponse = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/khachhang/{khachHang.MaKhachHang}");
            var lichHens = lichHenResponse.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<LichHenDTO>>(await lichHenResponse.Content.ReadAsStringAsync()) ?? new List<LichHenDTO>()
                : new List<LichHenDTO>();

            // 4️⃣ Lấy lịch sử thanh toán (nếu có API)
            var thanhToanResponse = await client.GetAsync($"{_apiBaseUrl}/api/ThanhToan/khachhang/{khachHang.MaKhachHang}");
            var thanhToans = thanhToanResponse.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<ThanhToanDTO>>(await thanhToanResponse.Content.ReadAsStringAsync()) ?? new List<ThanhToanDTO>()
                : new List<ThanhToanDTO>();

            // 5️⃣ Lấy tất cả dịch vụ hiện có
            var dichVuResponse = await client.GetAsync($"{_apiBaseUrl}/api/DichVu/alldichvu?pageSize=100");
            var dichVus = dichVuResponse.IsSuccessStatusCode
                ? JsonConvert.DeserializeObject<List<DichVuDTO>>(await dichVuResponse.Content.ReadAsStringAsync()) ?? new List<DichVuDTO>()
                : new List<DichVuDTO>();

            // 6️⃣ Gộp dữ liệu vào ViewModel
            var model = new KhachHangDashboardViewModel
            {
                KhachHang = khachHang,
                ThuCungs = pets,
                LichHens = lichHens,
                ThanhToans = thanhToans,
                DichVus = dichVus // Thêm danh sách dịch vụ
            };

            return View(model); // hiển thị view khách hàng
        }

        // === Trang tạo hồ sơ khách hàng (nếu chưa có) ===
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(KhachHangDTO model)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            model.MaNguoiDung = int.Parse(userId);
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var json = JsonConvert.SerializeObject(model);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync($"{_apiBaseUrl}/api/Khachhang/addkhachhang", content);

            if (response.IsSuccessStatusCode)
                return RedirectToAction("Index");

            ModelState.AddModelError("", "Không thể tạo hồ sơ. Vui lòng thử lại.");
            return View(model);
        }
    }

    // === ViewModel cho view KhachHang ===
    public class KhachHangDashboardViewModel
    {
        public KhachHangDTO KhachHang { get; set; }
        public List<ThuCungDTO> ThuCungs { get; set; }
        public List<LichHenDTO> LichHens { get; set; }
        public List<ThanhToanDTO> ThanhToans { get; set; }
        public List<DichVuDTO> DichVus { get; set; } // Thêm danh sách dịch vụ

        public KhachHangDashboardViewModel()
        {
            ThuCungs = new List<ThuCungDTO>();
            LichHens = new List<LichHenDTO>();
            ThanhToans = new List<ThanhToanDTO>();
            DichVus = new List<DichVuDTO>(); // Khởi tạo danh sách dịch vụ
        }
    }
}
