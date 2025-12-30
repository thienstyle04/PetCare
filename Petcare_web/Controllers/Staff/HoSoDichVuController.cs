using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Petcare_web.Models.DTO;
using PetCare_web.Models.DTO;
using System.Net.Http;

namespace PetCare.Controllers.Staff
{
    [Route("Staff/HoSoDichVu")]
    [Authorize(Roles = "NHANVIEN")]
    public class HoSoDichVuController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<HoSoDichVuController> _logger;
        private readonly string _apiBaseUrl;

        public HoSoDichVuController(
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<HoSoDichVuController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // GET: /Staff/HoSoDichVu
        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "Hồ sơ Dịch vụ";
            List<HoSoDichVuDTO>? danhSachHoSo = new();

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var response = await client.GetAsync("api/HoSoDichVu/allhosodichvu");

                response.EnsureSuccessStatusCode();
                danhSachHoSo = await response.Content.ReadFromJsonAsync<List<HoSoDichVuDTO>>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách hồ sơ dịch vụ");
                TempData["ErrorMessage"] = "Không thể tải danh sách hồ sơ dịch vụ.";
            }

            return View(danhSachHoSo);
        }

        // GET: /Staff/HoSoDichVu/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            ViewData["Title"] = "Chi tiết Hồ sơ Dịch vụ";

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var response = await client.GetAsync($"api/HoSoDichVu/gethosodichvu_id/{id}");
                response.EnsureSuccessStatusCode();

                var hoSo = await response.Content.ReadFromJsonAsync<HoSoDichVuDTO>();
                return View(hoSo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải chi tiết hồ sơ dịch vụ ID: {Id}", id);
                TempData["ErrorMessage"] = "Không thể tải thông tin chi tiết.";
                return RedirectToAction("Index");
            }
        }

        // GET: /Staff/HoSoDichVu/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            ViewData["Title"] = "Tạo hồ sơ dịch vụ mới";

            // Lấy MaNguoiDung từ session (đã lưu dưới dạng string)
            var userIdString = HttpContext.Session.GetString("UserId");
            _logger.LogInformation("=== CREATE FORM DEBUG ===");
            _logger.LogInformation("MaNguoiDung from session (string): {UserId}", userIdString);
            
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int maNguoiDung))
            {
                _logger.LogWarning("No UserId in session, redirecting to Login");
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");

                // Lấy MaNhanVien từ MaNguoiDung
                _logger.LogInformation("Getting MaNhanVien from MaNguoiDung: {MaNguoiDung}", maNguoiDung);
                var employeeResponse = await client.GetAsync($"api/NhanVien/getEmployeeIdByUserId/{maNguoiDung}");
                
                if (!employeeResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get MaNhanVien: {StatusCode}", employeeResponse.StatusCode);
                    TempData["ErrorMessage"] = "Không thể lấy thông tin nhân viên từ hệ thống.";
                    return RedirectToAction("Index", "StaffDashboard");
                }

                var maNhanVien = await employeeResponse.Content.ReadFromJsonAsync<int>();
                _logger.LogInformation("MaNhanVien resolved: {MaNhanVien}", maNhanVien);

                // Lấy danh sách lịch hẹn (chỉ lịch hẹn đã xác nhận, chưa có hồ sơ)
                _logger.LogInformation("Calling API: api/LichHen/alllichhen?pageSize=1000");
                var lichHenResponse = await client.GetAsync("api/LichHen/alllichhen?pageSize=1000");
                _logger.LogInformation("LichHen API Status: {StatusCode}", lichHenResponse.StatusCode);
                
                lichHenResponse.EnsureSuccessStatusCode();
                var lichHenList = await lichHenResponse.Content.ReadFromJsonAsync<List<LichHenDTO>>();
                _logger.LogInformation("Total LichHen received: {Count}", lichHenList?.Count ?? 0);

                // Lọc lịch hẹn: trạng thái "Đã xác nhận" hoặc "Hoàn thành"
                var availableLichHen = lichHenList?
                    .Where(lh => lh.TrangThai == "Đã xác nhận" || lh.TrangThai == "Hoàn thành")
                    .OrderByDescending(lh => lh.NgayGioHen)
                    .ToList() ?? new List<LichHenDTO>();

                _logger.LogInformation("Filtered LichHen (Đã xác nhận/Hoàn thành): {Count}", availableLichHen.Count);
                
                if (availableLichHen.Any())
                {
                    _logger.LogInformation("Sample LichHen: ID={Id}, TrangThai={TrangThai}", 
                        availableLichHen.First().MaLichHen, 
                        availableLichHen.First().TrangThai);
                }
                else
                {
                    _logger.LogWarning("No available LichHen after filtering!");
                }

                ViewBag.LichHenList = availableLichHen;
                ViewBag.MaNhanVien = maNhanVien;
                ViewBag.MaNguoiDung = maNguoiDung;

                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải dữ liệu cho form tạo hồ sơ");
                TempData["ErrorMessage"] = "Không thể tải dữ liệu. Vui lòng thử lại.";
                ViewBag.LichHenList = new List<LichHenDTO>();
                ViewBag.MaNhanVien = 0;
                return View();
            }
        }

        // POST: /Staff/HoSoDichVu/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HoSoDichVuDTO dto, IFormFile? AnhTruocFile, IFormFile? AnhSauFile)
        {
            _logger.LogInformation("=== POST CREATE CALLED ===");
            _logger.LogInformation("MaLichHen from form: {MaLichHen}", dto.MaLichHen);
            
            // Lấy MaNguoiDung từ session (đã lưu dưới dạng string)
            var userIdString = HttpContext.Session.GetString("UserId");
            _logger.LogInformation("MaNguoiDung from session (string): {UserId}", userIdString);
            
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int maNguoiDung))
            {
                _logger.LogWarning("No UserId in session during POST, redirecting to Login");
                TempData["ErrorMessage"] = "Không tìm thấy thông tin nhân viên. Vui lòng đăng nhập lại.";
                return RedirectToAction("Login", "Account");
            }

            int maNhanVien;
            
            try
            {
                // Lấy MaNhanVien từ MaNguoiDung
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var employeeResponse = await client.GetAsync($"api/NhanVien/getEmployeeIdByUserId/{maNguoiDung}");
                
                if (!employeeResponse.IsSuccessStatusCode)
                {
                    _logger.LogError("Failed to get MaNhanVien during POST: {StatusCode}", employeeResponse.StatusCode);
                    TempData["ErrorMessage"] = "Không thể xác định thông tin nhân viên.";
                    return RedirectToAction("Index");
                }

                maNhanVien = await employeeResponse.Content.ReadFromJsonAsync<int>();
                _logger.LogInformation("MaNhanVien resolved for POST: {MaNhanVien}", maNhanVien);

                // Gán MaNhanVien đã resolve
                dto.MaNhanVien = maNhanVien;
                dto.NgayTao = DateTime.Now;

                // Upload ảnh trước (nếu có)
                if (AnhTruocFile != null && AnhTruocFile.Length > 0)
                {
                    _logger.LogInformation("Uploading AnhTruoc: {FileName}", AnhTruocFile.FileName);
                    var uploadedPath = await UploadImageAsync(AnhTruocFile, "truoc");
                    if (!string.IsNullOrEmpty(uploadedPath))
                    {
                        dto.AnhTruoc = uploadedPath;
                        _logger.LogInformation("AnhTruoc uploaded successfully: {Path}", uploadedPath);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to upload AnhTruoc");
                    }
                }

                // Upload ảnh sau (nếu có)
                if (AnhSauFile != null && AnhSauFile.Length > 0)
                {
                    _logger.LogInformation("Uploading AnhSau: {FileName}", AnhSauFile.FileName);
                    var uploadedPath = await UploadImageAsync(AnhSauFile, "sau");
                    if (!string.IsNullOrEmpty(uploadedPath))
                    {
                        dto.AnhSau = uploadedPath;
                        _logger.LogInformation("AnhSau uploaded successfully: {Path}", uploadedPath);
                    }
                    else
                    {
                        _logger.LogWarning("Failed to upload AnhSau");
                    }
                }

                _logger.LogInformation("ModelState.IsValid: {IsValid}", ModelState.IsValid);
                if (!ModelState.IsValid)
                {
                    _logger.LogWarning("ModelState invalid. Errors: {Errors}", 
                        string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                    // Reload dropdown data
                    await LoadCreateViewData();
                    ViewBag.MaNhanVien = maNhanVien;
                    return View(dto);
                }

                _logger.LogInformation("Calling API: api/HoSoDichVu/themhosodichvu");
                
                var response = await client.PostAsJsonAsync("api/HoSoDichVu/themhosodichvu", dto);
                _logger.LogInformation("API Response Status: {StatusCode}", response.StatusCode);
                
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("SUCCESS: HoSoDichVu created");
                    TempData["SuccessMessage"] = "Tạo hồ sơ dịch vụ thành công!";
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("API returned error: {StatusCode} - {Content}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = $"Không thể tạo hồ sơ dịch vụ: {response.StatusCode}";
                }
                
                // If we reach here, API call failed
                await LoadCreateViewData();
                ViewBag.MaNhanVien = maNhanVien;
                return View(dto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tạo hồ sơ dịch vụ");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi tạo hồ sơ dịch vụ.";
                await LoadCreateViewData();
                ViewBag.MaNhanVien = 0;
                return View(dto);
            }
        }

        // Helper method to load dropdown data
        private async Task LoadCreateViewData()
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                var lichHenResponse = await client.GetAsync("api/LichHen/alllichhen?pageSize=1000");
                
                if (lichHenResponse.IsSuccessStatusCode)
                {
                    var lichHenList = await lichHenResponse.Content.ReadFromJsonAsync<List<LichHenDTO>>();
                    var availableLichHen = lichHenList?
                        .Where(lh => lh.TrangThai == "Đã xác nhận" || lh.TrangThai == "Hoàn thành")
                        .OrderByDescending(lh => lh.NgayGioHen)
                        .ToList() ?? new List<LichHenDTO>();
                    
                    ViewBag.LichHenList = availableLichHen;
                }
                else
                {
                    ViewBag.LichHenList = new List<LichHenDTO>();
                }
            }
            catch
            {
                ViewBag.LichHenList = new List<LichHenDTO>();
            }
        }

        // Helper method to upload image via API
        private async Task<string?> UploadImageAsync(IFormFile file, string prefix)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");
                
                // Tạo multipart form data
                using var content = new MultipartFormDataContent();
                using var fileStream = file.OpenReadStream();
                using var streamContent = new StreamContent(fileStream);
                
                // Thêm file vào form
                streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(streamContent, "File", file.FileName);
                
                // Thêm metadata
                var timestamp = DateTime.Now.ToString("yyyyMMddHHmmss");
                var fileName = $"hosodichvu_{prefix}_{timestamp}_{file.FileName}";
                content.Add(new StringContent(fileName), "FileName");
                content.Add(new StringContent($"Ảnh {prefix} của hồ sơ dịch vụ"), "FileDescription");
                
                _logger.LogInformation("Uploading to API: api/Images/Upload");
                var response = await client.PostAsync("api/Images/Upload", content);
                
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<ImageUploadResponseDTO>();
                    _logger.LogInformation("Upload success: {FilePath}", result?.FilePath);
                    return result?.FilePath;
                }
                else
                {
                    var error = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Upload failed: {StatusCode} - {Error}", response.StatusCode, error);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception during image upload");
                return null;
            }
        }
    }
}

// DTO cho response của upload image
public class ImageUploadResponseDTO
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string? FileDescription { get; set; }
    public string FileExtension { get; set; } = string.Empty;
    public long FileSizeInBytes { get; set; }
    public string FilePath { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
    public int? MaThuCung { get; set; }
}
