using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Petcare_web.Models.DTO;
using System.Net.Http.Headers;
using System.Text;

namespace Petcare_web.Controllers.Khachhang
{
    public class CustomerPaymentsController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string _apiBaseUrl = "https://localhost:7053";
        private readonly ILogger<CustomerPaymentsController> _logger;

        public CustomerPaymentsController(IHttpClientFactory httpClientFactory, ILogger<CustomerPaymentsController> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        // GET: CustomerPayments/Index - Danh sách lịch hẹn cần thanh toán
        public async Task<IActionResult> Index()
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy thông tin khách hàng
                var maNguoiDung = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy thông tin khách hàng
                var customerResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{maNguoiDung}");
                if (!customerResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login", "Account");
                }

                var customerJson = await customerResponse.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<KhachHangDTO>(customerJson);
                
                if (customer == null)
                {
                    return RedirectToAction("Login", "Account");
                }
                
                _logger.LogInformation("Lấy thông tin khách hàng ID: {CustomerId}", customer.MaKhachHang);

                // Lấy danh sách lịch hẹn đã hoàn thành của khách hàng
                var appointmentsResponse = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/khachhang/{customer.MaKhachHang}");
                if (!appointmentsResponse.IsSuccessStatusCode)
                {
                    ViewBag.ErrorMessage = "Không thể tải danh sách lịch hẹn";
                    return View(new List<LichHenDTO>());
                }

                var appointmentsJson = await appointmentsResponse.Content.ReadAsStringAsync();
                var allAppointments = JsonConvert.DeserializeObject<List<LichHenDTO>>(appointmentsJson);

                // Lọc các lịch hẹn đã hoàn thành
                var completedAppointments = allAppointments?
                    .Where(a => a.TrangThai == "Hoàn thành")
                    .ToList() ?? new List<LichHenDTO>();

                _logger.LogInformation("Tìm thấy {Count} lịch hẹn đã hoàn thành", completedAppointments.Count);

                // Lấy danh sách thanh toán
                var paymentsResponse = await client.GetAsync($"{_apiBaseUrl}/api/ThanhToan/khachhang/{customer.MaKhachHang}");
                List<ThanhToanDTO> payments = new List<ThanhToanDTO>();
                
                if (paymentsResponse.IsSuccessStatusCode)
                {
                    var paymentsJson = await paymentsResponse.Content.ReadAsStringAsync();
                    payments = JsonConvert.DeserializeObject<List<ThanhToanDTO>>(paymentsJson) ?? new List<ThanhToanDTO>();
                }

                // Lọc các lịch hẹn chưa thanh toán
                var unpaidAppointments = completedAppointments
                    .Where(a => !payments.Any(p => p.MaLichHen == a.MaLichHen && p.TrangThai == "DaThanhToan"))
                    .ToList();

                ViewBag.Payments = payments;
                ViewBag.CustomerName = customer?.HoTen ?? "Khách hàng";

                return View(unpaidAppointments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải danh sách thanh toán");
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tải dữ liệu";
                return View(new List<LichHenDTO>());
            }
        }

        // GET: CustomerPayments/Create/{maLichHen} - Form thanh toán
        public async Task<IActionResult> Create(int maLichHen)
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy thông tin lịch hẹn
                var appointmentResponse = await client.GetAsync($"{_apiBaseUrl}/api/LichHen/get_lichhen_id/{maLichHen}");
                if (!appointmentResponse.IsSuccessStatusCode)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy lịch hẹn";
                    return RedirectToAction("Index");
                }

                var appointmentJson = await appointmentResponse.Content.ReadAsStringAsync();
                var appointment = JsonConvert.DeserializeObject<LichHenDTO>(appointmentJson);

                if (appointment == null || appointment.TrangThai != "Hoàn thành")
                {
                    TempData["ErrorMessage"] = "Lịch hẹn chưa hoàn thành hoặc không hợp lệ";
                    return RedirectToAction("Index");
                }

                ViewBag.Appointment = appointment;
                return View();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải form thanh toán cho lịch hẹn {AppointmentId}", maLichHen);
                TempData["ErrorMessage"] = "Đã xảy ra lỗi";
                return RedirectToAction("Index");
            }
        }

        // POST: CustomerPayments/Create - Xử lý thanh toán
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ThanhToanRequestDTO model)
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Gửi yêu cầu tạo thanh toán
                var json = JsonConvert.SerializeObject(model);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                
                _logger.LogInformation("Tạo thanh toán cho lịch hẹn {AppointmentId}, số tiền: {Amount}, phương thức: {Method}", 
                    model.MaLichHen, model.SoTien, model.PhuongThuc);

                var response = await client.PostAsync($"{_apiBaseUrl}/api/ThanhToan/themthanhtoan", content);

                if (response.IsSuccessStatusCode)
                {
                    TempData["SuccessMessage"] = "Thanh toán thành công!";
                    _logger.LogInformation("Thanh toán thành công cho lịch hẹn {AppointmentId}", model.MaLichHen);
                    return RedirectToAction("Index");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Thanh toán thất bại: {StatusCode}, {Error}", response.StatusCode, errorContent);
                    TempData["ErrorMessage"] = "Thanh toán thất bại. Vui lòng thử lại.";
                    return RedirectToAction("Create", new { maLichHen = model.MaLichHen });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý thanh toán");
                TempData["ErrorMessage"] = "Đã xảy ra lỗi khi xử lý thanh toán";
                return RedirectToAction("Create", new { maLichHen = model.MaLichHen });
            }
        }

        // GET: CustomerPayments/History - Lịch sử thanh toán
        public async Task<IActionResult> History()
        {
            try
            {
                var token = HttpContext.Session.GetString("JwtToken");
                if (string.IsNullOrEmpty(token))
                {
                    return RedirectToAction("Login", "Account");
                }

                var maNguoiDung = int.Parse(HttpContext.Session.GetString("UserId") ?? "0");
                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Lấy thông tin khách hàng
                var customerResponse = await client.GetAsync($"{_apiBaseUrl}/api/Khachhang/user/{maNguoiDung}");
                if (!customerResponse.IsSuccessStatusCode)
                {
                    return RedirectToAction("Login", "Account");
                }

                var customerJson = await customerResponse.Content.ReadAsStringAsync();
                var customer = JsonConvert.DeserializeObject<KhachHangDTO>(customerJson);
                
                if (customer == null)
                {
                    return RedirectToAction("Login", "Account");
                }

                // Lấy lịch sử thanh toán
                var paymentsResponse = await client.GetAsync($"{_apiBaseUrl}/api/ThanhToan/khachhang/{customer.MaKhachHang}");
                
                List<ThanhToanDTO> payments = new List<ThanhToanDTO>();
                if (paymentsResponse.IsSuccessStatusCode)
                {
                    var paymentsJson = await paymentsResponse.Content.ReadAsStringAsync();
                    _logger.LogInformation("API Response: {Json}", paymentsJson);
                    
                    payments = JsonConvert.DeserializeObject<List<ThanhToanDTO>>(paymentsJson) ?? new List<ThanhToanDTO>();
                    
                    _logger.LogInformation("Số lượng thanh toán: {Count}", payments.Count);
                    if (payments.Any())
                    {
                        foreach (var p in payments)
                        {
                            _logger.LogInformation("Payment: MaTT={MaTT}, MaLichHen={MaLichHen}, SoTien={SoTien}, PhuongThuc={PT}, TrangThai={TT}", 
                                p.MaThanhToan, p.MaLichHen, p.SoTien, p.PhuongThuc, p.TrangThai);
                        }
                    }
                }
                else
                {
                    _logger.LogWarning("API trả về lỗi: {Status}", paymentsResponse.StatusCode);
                }

                ViewBag.CustomerName = customer?.HoTen ?? "Khách hàng";
                return View(payments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi tải lịch sử thanh toán");
                ViewBag.ErrorMessage = "Đã xảy ra lỗi khi tải lịch sử thanh toán";
                return View(new List<ThanhToanDTO>());
            }
        }
    }
}
