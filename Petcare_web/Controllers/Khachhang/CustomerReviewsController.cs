using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Petcare_web.Models.DTO;
using System.Net.Http.Headers;
using System.Text;

namespace Petcare_web.Controllers.Khachhang
{
    public class CustomerReviewsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly string _apiBaseUrl;
        private readonly ILogger<CustomerReviewsController> _logger;

        public CustomerReviewsController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<CustomerReviewsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _apiBaseUrl = _configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7053";
            _logger = logger;
        }

        // GET: CustomerReviews/Index - Hiển thị danh sách dịch vụ đã sử dụng
        public async Task<IActionResult> Index()
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                var token = HttpContext.Session.GetString("JwtToken");

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy thông tin khách hàng
                var khResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{userId}");
                if (!khResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không thể tải thông tin khách hàng";
                    return View(new List<LichHenReviewDTO>());
                }

                var khJson = await khResponse.Content.ReadAsStringAsync();
                var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(khJson);

                // Lấy danh sách lịch hẹn đã hoàn thành
                var lichHenResponse = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/khachhang/{khachHang!.MaKhachHang}");
                if (!lichHenResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không thể tải danh sách lịch hẹn";
                    return View(new List<LichHenReviewDTO>());
                }

                var lichHenJson = await lichHenResponse.Content.ReadAsStringAsync();
                var lichHenList = JsonConvert.DeserializeObject<List<LichHenDTO>>(lichHenJson);

                // Lấy danh sách đánh giá của khách hàng (sử dụng endpoint mới)
                var danhGiaResponse = await client.GetAsync($"{_apiBaseUrl}/api/DanhGia/khachhang/{khachHang.MaKhachHang}");
                List<DanhGiaDTO> existingReviews = new List<DanhGiaDTO>();
                
                if (danhGiaResponse.IsSuccessStatusCode)
                {
                    var danhGiaJson = await danhGiaResponse.Content.ReadAsStringAsync();
                    existingReviews = JsonConvert.DeserializeObject<List<DanhGiaDTO>>(danhGiaJson) ?? new List<DanhGiaDTO>();
                    _logger.LogInformation("Found {Count} existing reviews for customer {MaKhachHang}", existingReviews.Count, khachHang.MaKhachHang);
                }

                // Lấy danh sách thanh toán để kiểm tra lịch hẹn đã thanh toán
                var thanhToanResponse = await client.GetAsync($"{_apiBaseUrl}/api/ThanhToan/khachhang/{khachHang.MaKhachHang}");
                List<int> paidAppointmentIds = new List<int>();
                if (thanhToanResponse.IsSuccessStatusCode)
                {
                    var thanhToanJson = await thanhToanResponse.Content.ReadAsStringAsync();
                    var payments = JsonConvert.DeserializeObject<List<dynamic>>(thanhToanJson);
                    paidAppointmentIds = payments?.Select(p => (int)p.maLichHen).ToList() ?? new List<int>();
                }

                // Lọc lịch hẹn đã hoàn thành HOẶC đã thanh toán
                var completedAppointments = lichHenList?
                    .Where(lh => lh.TrangThai == "Đã hoàn thành" || paidAppointmentIds.Contains(lh.MaLichHen))
                    .Select(lh => new LichHenReviewDTO
                    {
                        MaLichHen = lh.MaLichHen,
                        NgayHen = lh.NgayGioHen.Date,
                        GioHen = lh.NgayGioHen.TimeOfDay,
                        TenDichVu = lh.TenDichVu,
                        TenThuCung = lh.TenThuCung,
                        GiaDichVu = lh.TongTien ?? 0,
                        DaDanhGia = existingReviews.Any(r => r.MaLichHen == lh.MaLichHen),
                        DiemSo = existingReviews.FirstOrDefault(r => r.MaLichHen == lh.MaLichHen)?.DiemSo,
                        BinhLuan = existingReviews.FirstOrDefault(r => r.MaLichHen == lh.MaLichHen)?.BinhLuan
                    })
                    .OrderByDescending(lh => lh.NgayHen)
                    .ToList() ?? new List<LichHenReviewDTO>();

                return View(completedAppointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách đánh giá");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tải danh sách";
                return View(new List<LichHenReviewDTO>());
            }
        }

        // GET: CustomerReviews/Create/{maLichHen}
        public async Task<IActionResult> Create(int maLichHen)
        {
            try
            {
                var userId = HttpContext.Session.GetString("UserId");
                var token = HttpContext.Session.GetString("JwtToken");

                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy thông tin khách hàng
                var khResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{userId}");
                if (!khResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không thể xác thực khách hàng";
                    return RedirectToAction("Index");
                }

                var khJson = await khResponse.Content.ReadAsStringAsync();
                var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(khJson);

                // Lấy danh sách lịch hẹn của khách hàng
                var lichHenResponse = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/khachhang/{khachHang!.MaKhachHang}");
                if (!lichHenResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không thể tải thông tin lịch hẹn";
                    return RedirectToAction("Index");
                }

                var lichHenJson = await lichHenResponse.Content.ReadAsStringAsync();
                var lichHenList = JsonConvert.DeserializeObject<List<LichHenDTO>>(lichHenJson);

                // Tìm lịch hẹn theo mã
                var lichHen = lichHenList?.FirstOrDefault(lh => lh.MaLichHen == maLichHen);
                if (lichHen == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch hẹn";
                    return RedirectToAction("Index");
                }

                var reviewDto = new DanhGiaDTO
                {
                    MaLichHen = maLichHen,
                    MaKhachHang = khachHang.MaKhachHang,
                    DiemSo = 0, // Không chọn mặc định, để user tự chọn
                    BinhLuan = ""
                };

                ViewBag.LichHen = lichHen;
                return View(reviewDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form đánh giá");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi";
                return RedirectToAction("Index");
            }
        }

        // POST: CustomerReviews/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DanhGiaDTO model)
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Log validation errors
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("Validation failed for review creation");
                    foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
                    {
                        _logger.LogWarning("Validation error: {Error}", error.ErrorMessage);
                    }

                    // Reload lịch hẹn info nếu validation fail
                    var client = _httpClientFactory.CreateClient();
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    
                    var userId = HttpContext.Session.GetString("UserId");
                    var khResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{userId}");
                    if (khResponse.IsSuccessStatusCode)
                    {
                        var khJson = await khResponse.Content.ReadAsStringAsync();
                        var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(khJson);
                        
                        var lichHenResponse = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/khachhang/{khachHang!.MaKhachHang}");
                        if (lichHenResponse.IsSuccessStatusCode)
                        {
                            var lichHenJson = await lichHenResponse.Content.ReadAsStringAsync();
                            var lichHenList = JsonConvert.DeserializeObject<List<LichHenDTO>>(lichHenJson);
                            var lichHen = lichHenList?.FirstOrDefault(lh => lh.MaLichHen == model.MaLichHen);
                            ViewBag.LichHen = lichHen;
                        }
                    }

                    TempData["ErrorMessage"] = "Vui lòng chọn mức đánh giá từ 1 đến 5 sao.";
                    return View(model);
                }

                var httpClient = _httpClientFactory.CreateClient();
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Tạo object theo đúng Domain model format (navigation properties nullable)
                var danhGiaRequest = new
                {
                    MaLichHen = model.MaLichHen,
                    MaKhachHang = model.MaKhachHang,
                    DiemSo = model.DiemSo,
                    BinhLuan = model.BinhLuan ?? "",
                    NgayTao = DateTime.Now
                };

                var jsonContent = JsonConvert.SerializeObject(danhGiaRequest);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                _logger.LogInformation("Tạo đánh giá cho lịch hẹn {MaLichHen} với điểm số {DiemSo}", model.MaLichHen, model.DiemSo);

                var response = await httpClient.PostAsync($"{_apiBaseUrl}/api/DanhGia", content);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Đánh giá thành công");
                    TempData["SuccessMessage"] = "Cảm ơn bạn đã đánh giá dịch vụ!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Tạo đánh giá thất bại. Status: {StatusCode}, Error: {Error}", 
                        response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Không thể gửi đánh giá. Vui lòng thử lại.";
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo đánh giá");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi gửi đánh giá";
                return RedirectToAction("Index");
            }
        }
    }
}
