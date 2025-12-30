using Microsoft.AspNetCore.Mvc;
using Petcare_web.Models.DTO;
using System.Net.Http.Headers;
using System.Net.Http;

namespace Petcare_web.Controllers.Staff
{
    [Route("Staff/Services")]
    public class StaffServicesController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl;

        public StaffServicesController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _apiBaseUrl = configuration.GetValue<string>("ApiSettings:BaseUrl");
        }

        // GET: /Staff/Services/Index
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var services = new List<DichVuDTO>();
            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");

                // lấy danh sách dịch vụ
                var response = await client.GetAsync($"{_apiBaseUrl}/api/DichVu/alldichvu");
                response.EnsureSuccessStatusCode();

                services = await response.Content.ReadFromJsonAsync<List<DichVuDTO>>() ?? new();

                // duyệt từng dịch vụ -> gọi API ảnh
                foreach (var svc in services)
                {
                    var imgResponse = await client.GetAsync($"{_apiBaseUrl}/api/Image/GetByDichVu/{svc.MaDichVu}");
                    if (imgResponse.IsSuccessStatusCode)
                    {
                        // giả sử API trả về string URL ảnh
                        var imgUrl = await imgResponse.Content.ReadAsStringAsync();
                        svc.ImageUrl = imgUrl;
                    }
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Không thể tải danh sách dịch vụ: {ex.Message}";
            }

            return View(services);
        }
        // GET: /Staff/Services/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Staff/Services/Create
        [HttpPost("Create")]
        public async Task<IActionResult> Create(DichVuDTO request, IFormFile? imageFile)
        {
            if (!ModelState.IsValid)
            {
                return View(request);
            }

            try
            {
                var client = _httpClientFactory.CreateClient("PetCareApiClient");

                // 1. Gửi yêu cầu thêm dịch vụ
                var response = await client.PostAsJsonAsync($"{_apiBaseUrl}/api/DichVu/themdichvu", request);

                if (!response.IsSuccessStatusCode)
                {
                    var err = await response.Content.ReadAsStringAsync();
                    ViewData["ErrorMessage"] = $"Không thể thêm dịch vụ: {err}";
                    return View(request);
                }

                // lấy MaDichVu vừa tạo
                var createdService = await response.Content.ReadFromJsonAsync<DichVuDTO>();
                var maDichVu = createdService?.MaDichVu;

                // 2. Nếu có upload ảnh → gọi API upload
                if (imageFile != null && maDichVu.HasValue)
                {
                    var formContent = new MultipartFormDataContent();
                    var streamContent = new StreamContent(imageFile.OpenReadStream());
                    streamContent.Headers.ContentType = new MediaTypeHeaderValue(imageFile.ContentType);

                    formContent.Add(streamContent, "File", imageFile.FileName);
                    formContent.Add(new StringContent(imageFile.FileName), "FileName");
                    formContent.Add(new StringContent($"Ảnh dịch vụ {request.TenDichVu}"), "FileDescription");
                    formContent.Add(new StringContent(maDichVu.Value.ToString()), "MaDichVu");

                    var imgResponse = await client.PostAsync($"{_apiBaseUrl}/api/Image/upload", formContent);
                    if (!imgResponse.IsSuccessStatusCode)
                    {
                        TempData["WarningMessage"] = "Dịch vụ đã thêm nhưng upload ảnh thất bại.";
                    }
                }

                TempData["SuccessMessage"] = "Thêm dịch vụ thành công!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewData["ErrorMessage"] = $"Lỗi hệ thống: {ex.Message}";
                return View(request);
            }
        }

    }
}
