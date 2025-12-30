using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Petcare_web.Models.DTO;
using System.Net.Http.Headers;
using System.Text;
using System.Diagnostics;

namespace Petcare_web.Controllers
{
    [Authorize(Roles = "KHACHHANG")]
    public class CustomerAppointmentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public CustomerAppointmentsController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl") ?? "https://localhost:7179";
        }

        // 📅 Danh sách lịch hẹn
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
                    return View("Index", new List<LichHenDTO>());
                }

                var khData = await khResponse.Content.ReadAsStringAsync();
                var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(khData);
                
                if (khachHang == null)
                {
                    return View("Index", new List<LichHenDTO>());
                }

                // 2. Lấy danh sách lịch hẹn theo MaKhachHang
                var response = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/khachhang/{khachHang.MaKhachHang}");
                var lichHens = new List<LichHenDTO>();

                if (response.IsSuccessStatusCode)
                {
                    lichHens = JsonConvert.DeserializeObject<List<LichHenDTO>>(
                        await response.Content.ReadAsStringAsync()) ?? new List<LichHenDTO>();
                }

                return View("Index", lichHens);
            }
            catch (Exception)
            {
                return View("Index", new List<LichHenDTO>());
            }
        }

        // ➕ Form thêm lịch hẹn
        [HttpGet]
        public async Task<IActionResult> Add()
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
                    ViewBag.Pets = new List<ThuCungDTO>();
                    ViewBag.Services = new List<DichVuDTO>();
                    return View();
                }

                var khData = await khResponse.Content.ReadAsStringAsync();
                var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(khData);
                
                if (khachHang == null)
                {
                    ViewBag.Pets = new List<ThuCungDTO>();
                    ViewBag.Services = new List<DichVuDTO>();
                    return View();
                }

                // 2. Lấy danh sách thú cưng theo MaKhachHang
                var petsResponse = await client.GetAsync($"{_apiBaseUrl}/api/ThuCung/khachhang/{khachHang.MaKhachHang}");
                if (petsResponse.IsSuccessStatusCode)
                {
                    var petsJson = await petsResponse.Content.ReadAsStringAsync();
                    System.Diagnostics.Debug.WriteLine($"Pets JSON Response: {petsJson}");
                    
                    var pets = JsonConvert.DeserializeObject<List<ThuCungDTO>>(petsJson) ?? new List<ThuCungDTO>();
                    System.Diagnostics.Debug.WriteLine($"Pets Count after deserialize: {pets.Count}");
                    
                    if (pets.Any())
                    {
                        System.Diagnostics.Debug.WriteLine($"First pet: MaThuCung={pets[0].MaThuCung}, TenThuCung={pets[0].TenThuCung}, Loai={pets[0].Loai}");
                    }
                    
                    ViewBag.Pets = pets;
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"Failed to get pets. Status: {petsResponse.StatusCode}");
                    ViewBag.Pets = new List<ThuCungDTO>();
                }

                // 3. Lấy danh sách dịch vụ
                var servicesResponse = await client.GetAsync($"{_apiBaseUrl}/api/DichVu/alldichvu");
                if (servicesResponse.IsSuccessStatusCode)
                {
                    var services = JsonConvert.DeserializeObject<List<DichVuDTO>>(
                        await servicesResponse.Content.ReadAsStringAsync()) ?? new List<DichVuDTO>();
                    ViewBag.Services = services;
                }
                else
                {
                    ViewBag.Services = new List<DichVuDTO>();
                }
            }
            catch (Exception)
            {
                ViewBag.Pets = new List<ThuCungDTO>();
                ViewBag.Services = new List<DichVuDTO>();
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(LichHenRequestDTO request)
        {
            var userId = HttpContext.Session.GetString("UserId");
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            // 🔍 Debug: Kiểm tra request data
            Console.WriteLine($"=== POST Add được gọi ===");
            Console.WriteLine($"MaThuCung: {request.MaThuCung}");
            Console.WriteLine($"MaDichVu: {request.MaDichVu}");
            Console.WriteLine($"NgayHen: {request.NgayHen}");
            Console.WriteLine($"GioHen: {request.GioHen}");
            Console.WriteLine($"GhiChu: {request.GhiChu}");
            Console.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");
            
            if (!ModelState.IsValid)
            {
                Console.WriteLine("❌ ModelState có lỗi:");
                foreach (var error in ModelState)
                {
                    Console.WriteLine($"  - Key: {error.Key}");
                    foreach (var err in error.Value.Errors)
                    {
                        Console.WriteLine($"    Error: {err.ErrorMessage}");
                    }
                }
                
                // ✅ QUAN TRỌNG: Phải reload lại ViewBag trước khi return View
                await LoadPetsAndServicesForView(userId, token);
                return View(request);
            }

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // 1. Lấy MaKhachHang từ MaNguoiDung
                var khResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{userId}");
                
                if (!khResponse.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", "Không thể xác định thông tin khách hàng.");
                    await LoadPetsAndServicesForView(userId, token);
                    return View(request);
                }

                var khData = await khResponse.Content.ReadAsStringAsync();
                var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(khData);
                
                if (khachHang == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy thông tin khách hàng.");
                    await LoadPetsAndServicesForView(userId, token);
                    return View(request);
                }

                // 2. Gán MaKhachHang và xử lý thời gian
                request.MaKhachHang = khachHang.MaKhachHang;
                request.TrangThai = "Chờ xác nhận"; // Trạng thái mặc định

                Console.WriteLine($"=== Trước khi kết hợp NgayGioHen ===");
                Console.WriteLine($"NgayHen: {request.NgayHen}");
                Console.WriteLine($"GioHen: {request.GioHen}");

                // Kết hợp NgayHen và GioHen thành NgayGioHen
                if (!string.IsNullOrEmpty(request.NgayHen) && !string.IsNullOrEmpty(request.GioHen))
                {
                    var ngay = DateTime.Parse(request.NgayHen);
                    var gio = TimeSpan.Parse(request.GioHen);
                    request.NgayGioHen = ngay.Add(gio);
                }
                else if (!string.IsNullOrEmpty(request.NgayHen))
                {
                    request.NgayGioHen = DateTime.Parse(request.NgayHen);
                }

                Console.WriteLine($"NgayGioHen sau khi kết hợp: {request.NgayGioHen}");

                // 3. Gửi request tạo lịch hẹn
                var json = JsonConvert.SerializeObject(request);
                Console.WriteLine($"=== JSON gửi đi ===");
                Console.WriteLine(json);
                
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{_apiBaseUrl}/api/LichHen/themlichhen", content);

                if (response.IsSuccessStatusCode)
                    return RedirectToAction("Index");

                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"=== API trả về lỗi 400 ===");
                Console.WriteLine(errorContent);
                ModelState.AddModelError("", $"Không thể tạo lịch hẹn. Chi tiết: {errorContent}");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Lỗi: {ex.Message}");
            }

            return View(request);
        }

        // ✅ Method helper để load Pets và Services cho ViewBag
        private async Task LoadPetsAndServicesForView(string userId, string token)
        {
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
                // 1. Load Pets
                var khResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{userId}");
                if (khResponse.IsSuccessStatusCode)
                {
                    var khData = await khResponse.Content.ReadAsStringAsync();
                    var khachHang = JsonConvert.DeserializeObject<KhachHangDTO>(khData);
                    
                    if (khachHang != null)
                    {
                        var petResponse = await client.GetAsync($"{_apiBaseUrl}/api/ThuCung/khachhang/{khachHang.MaKhachHang}");
                        if (petResponse.IsSuccessStatusCode)
                        {
                            var petData = await petResponse.Content.ReadAsStringAsync();
                            var pets = JsonConvert.DeserializeObject<List<ThuCungDTO>>(petData);
                            ViewBag.Pets = pets ?? new List<ThuCungDTO>();
                        }
                    }
                }

                // 2. Load Services
                var serviceResponse = await client.GetAsync($"{_apiBaseUrl}/api/DichVu/alldichvu");
                if (serviceResponse.IsSuccessStatusCode)
                {
                    var serviceData = await serviceResponse.Content.ReadAsStringAsync();
                    var services = JsonConvert.DeserializeObject<List<DichVuDTO>>(serviceData);
                    ViewBag.Services = services ?? new List<DichVuDTO>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Lỗi khi load dữ liệu ViewBag: {ex.Message}");
                ViewBag.Pets = new List<ThuCungDTO>();
                ViewBag.Services = new List<DichVuDTO>();
            }
        }

        // 🔍 Chi tiết lịch hẹn
        [HttpGet]
        [Route("CustomerAppointments/Detail/{id}")]
        public async Task<IActionResult> Detail(int id)
        {
            Console.WriteLine($"=== Detail action được gọi với id={id} ===");
            
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            Console.WriteLine($"Gọi API: {_apiBaseUrl}/api/LichHen/get_lichhen_id/{id}");
            var response = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/get_lichhen_id/{id}");
            
            Console.WriteLine($"API response status: {response.StatusCode}");
            
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("API trả về lỗi, redirect về Index");
                return RedirectToAction("Index");
            }

            var json = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"API response data: {json}");
            
            var appointment = JsonConvert.DeserializeObject<LichHenDTO>(json);

            // Lấy hồ sơ dịch vụ nếu có (để hiển thị ảnh trước/sau)
            HoSoDichVuDTO? hoSoDichVu = null;
            if (appointment != null && appointment.TrangThai == "Hoàn thành")
            {
                try
                {
                    var hoSoResponse = await client.GetAsync($"{_apiBaseUrl}/api/HoSoDichVu/allhosodichvu?filterOn=MaLichHen&filterQuery={id}");
                    if (hoSoResponse.IsSuccessStatusCode)
                    {
                        var hoSoJson = await hoSoResponse.Content.ReadAsStringAsync();
                        var hoSoList = JsonConvert.DeserializeObject<List<HoSoDichVuDTO>>(hoSoJson);
                        hoSoDichVu = hoSoList?.FirstOrDefault();
                        Console.WriteLine($"Tìm thấy hồ sơ dịch vụ: {hoSoDichVu?.MaHoSo}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Lỗi khi lấy hồ sơ dịch vụ: {ex.Message}");
                }
            }

            ViewBag.HoSoDichVu = hoSoDichVu;
            return View("Detail", appointment);
        }

        // ❌ Hủy lịch hẹn
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var token = HttpContext.Session.GetString("JwtToken");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            try
            {
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Cập nhật trạng thái lịch hẹn thành "Đã hủy"
                var updateDto = new { TrangThai = "Đã hủy" };
                var content = new StringContent(JsonConvert.SerializeObject(updateDto), Encoding.UTF8, "application/json");
                
                var response = await client.PutAsync($"{_apiBaseUrl}/api/LichHen/update_lichhen_id/{id}", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Hủy lịch hẹn thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể hủy lịch hẹn. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction("Index");
        }
    }
}
