using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using PetCare.Repositories; // Đảm bảo bạn đã using repository interface
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class LichHenController : ControllerBase
{
    private readonly ILichHenRepository lichHenRepo;
    private readonly ILogger<LichHenController> _logger;

    public LichHenController(ILichHenRepository repo, ILogger<LichHenController> logger)
    {
        lichHenRepo = repo;
        _logger = logger;
    }

    // GET: api/LichHen/khachhang/{maKhachHang}
    [HttpGet("khachhang/{maKhachHang:int}")]
    [Authorize(Roles = "KHACHHANG, QUANTRI, NHANVIEN")]
    public async Task<IActionResult> GetByKhachHang([FromRoute] int maKhachHang)
    {
        _logger.LogInformation("Fetching appointments for customer ID: {CustomerId}", maKhachHang);
        
        var appointments = await lichHenRepo.GetAllAsync();
        var filtered = appointments.Where(a => a.MaKhachHang == maKhachHang).ToList();
        
        _logger.LogInformation("Found {Count} appointments for customer ID: {CustomerId}", filtered.Count, maKhachHang);
        return Ok(filtered);
    }

    // GET: api/LichHen/alllichhen (Không thay đổi)
    [HttpGet]
    [Route("alllichhen")]
    [Authorize(Roles = "QUANTRI, NHANVIEN")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? filterOn = null, [FromQuery] string? filterQuery = null,
        [FromQuery] string? sortBy = null, [FromQuery] bool isAscending = true,
        [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
    {
        var result = await lichHenRepo.GetAllAsync(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
        return Ok(result);
    }

    // GET: api/LichHen/{id}
    [HttpGet("get_lichhen_id/{id}")]
    [Authorize(Roles = "QUANTRI, NHANVIEN, KHACHHANG")] // ✅ Thêm KHACHHANG để xem chi tiết lịch hẹn
    public async Task<IActionResult> GetById(int id)
    {
        var result = await lichHenRepo.GetByIdAsync(id);
        if (result == null)
        {
            return NotFound(new { Message = $"Không tìm thấy lịch hẹn ID: {id}" });
        }
        return Ok(result);
    }

    // ✅ SỬA LỖI: Chấp nhận DTO và gọi đúng phương thức Repository
    // POST: api/LichHen/themlichhen
    [HttpPost]
    [Route("themlichhen")]
    [Authorize(Roles = "QUANTRI, NHANVIEN, KHACHHANG")] // ✅ Thêm KHACHHANG để cho phép đặt lịch
    public async Task<IActionResult> Create([FromBody] LichHenRequestDTO requestDto)
    {
        // 🔍 Debug: Log request data
        _logger.LogInformation($"=== API Create LichHen ===");
        _logger.LogInformation($"MaThuCung: {requestDto.MaThuCung}");
        _logger.LogInformation($"MaDichVu: {requestDto.MaDichVu}");
        _logger.LogInformation($"MaKhachHang: {requestDto.MaKhachHang}");
        _logger.LogInformation($"NgayGioHen: {requestDto.NgayGioHen}");
        _logger.LogInformation($"GhiChu: {requestDto.GhiChu}");
        
        if (!ModelState.IsValid)
        {
            _logger.LogWarning("❌ ModelState invalid:");
            foreach (var error in ModelState)
            {
                _logger.LogWarning($"  - Key: {error.Key}");
                foreach (var err in error.Value.Errors)
                {
                    _logger.LogWarning($"    Error: {err.ErrorMessage}");
                }
            }
            return BadRequest(ModelState);
        }

        // Gọi phương thức AddAsync của repository vốn đã được thiết kế để nhận DTO
        var result = await lichHenRepo.AddAsync(requestDto);

        if (result == null)
        {
            // Có thể do MaDichVu không hợp lệ
            return BadRequest("Không thể tạo lịch hẹn. Vui lòng kiểm tra lại thông tin dịch vụ.");
        }

        var resultDto = await lichHenRepo.GetByIdAsync(result.MaLichHen);
        return CreatedAtAction(nameof(GetById), new { id = result.MaLichHen }, resultDto);
    }

    // ✅ SỬA LỖI: Chấp nhận DTO và gọi đúng phương thức Repository
    // PUT: api/LichHen/{id}
    [HttpPut("update_lichhen_id/{id}")]
    [Authorize(Roles = "QUANTRI, NHANVIEN, KHACHHANG")] // ✅ Thêm KHACHHANG để cho phép hủy lịch
    public async Task<IActionResult> Update(int id, [FromBody] UpdateLichHenDTO updateDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Gọi phương thức UpdateAsync của repository vốn đã được thiết kế để nhận DTO
        var result = await lichHenRepo.UpdateAsync(id, updateDto);

        if (result == null)
        {
            return NotFound(new { Message = $"Không tìm thấy lịch hẹn ID: {id} để cập nhật." });
        }

        return Ok(result);
    }

    // DELETE: api/LichHen/{id} (Không thay đổi)
    [HttpDelete("delete_lichhen_id/{id}")]
    [Authorize(Roles = "QUANTRI")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await lichHenRepo.DeleteAsync(id);
        if (result == null)
        {
            return NotFound(new { Message = $"Không tìm thấy lịch hẹn ID: {id} để xóa." });
        }
        return Ok(result);
    }

    // GET: api/LichHen/nhanvien/{maNhanVien}
    [HttpGet("nhanvien/{maNhanVien:int}")]
    [Authorize(Roles = "NHANVIEN, QUANTRI")]
    public async Task<IActionResult> GetByNhanVien([FromRoute] int maNhanVien)
    {
        _logger.LogInformation("Fetching appointments for staff ID: {StaffId}", maNhanVien);
        
        var appointments = await lichHenRepo.GetAllAsync();
        var filtered = appointments.Where(a => a.MaNhanVien == maNhanVien).ToList();
        
        _logger.LogInformation("Found {Count} appointments for staff ID: {StaffId}", filtered.Count, maNhanVien);
        return Ok(filtered);
    }

    // PATCH: api/LichHen/{id}/status
    [HttpPatch("{id}/status")]
    [Authorize(Roles = "QUANTRI, NHANVIEN")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateStatusDTO statusDto)
    {
        _logger.LogInformation("Updating status for appointment ID: {AppointmentId} to {Status}", id, statusDto.TrangThai);
        
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var appointment = await lichHenRepo.GetByIdAsync(id);
        if (appointment == null)
        {
            return NotFound(new { Message = $"Không tìm thấy lịch hẹn ID: {id}" });
        }

        // ✅ Lấy MaNguoiDung từ token
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        int? maNhanVien = null;

        _logger.LogInformation("=== DEBUG UpdateStatus ===");
        _logger.LogInformation("User ID from token: {UserId}", userIdClaim);
        _logger.LogInformation("Is NHANVIEN role: {IsStaff}", User.IsInRole("NHANVIEN"));
        _logger.LogInformation("Status being set: {Status}", statusDto.TrangThai);

        // ✅ Nếu là nhân viên và đang xác nhận → gán nhân viên
        if (User.IsInRole("NHANVIEN") && statusDto.TrangThai == "Đã xác nhận" && !string.IsNullOrEmpty(userIdClaim))
        {
            _logger.LogInformation("Condition met: Staff confirming appointment");
            // Lấy MaNhanVien từ IdentityUserId (GUID)
            var nhanVienRepo = HttpContext.RequestServices.GetService<PetCare.Repositories.INhanVienRepositories>();
            if (nhanVienRepo != null)
            {
                _logger.LogInformation("NhanVienRepo found, looking up staff by IdentityUserId: {IdentityUserId}", userIdClaim);
                var nhanVien = nhanVienRepo.GetNhanVienByIdentityUserId(userIdClaim);
                if (nhanVien != null)
                {
                    maNhanVien = nhanVien.MaNhanVien;
                    _logger.LogInformation("✅ Assigning staff ID: {StaffId} to appointment ID: {AppointmentId}", maNhanVien, id);
                }
                else
                {
                    _logger.LogWarning("⚠️ Staff not found for IdentityUserId: {IdentityUserId}", userIdClaim);
                }
            }
            else
            {
                _logger.LogWarning("⚠️ NhanVienRepo is null");
            }
        }
        else
        {
            _logger.LogInformation("Condition NOT met for staff assignment");
        }

        // Tạo UpdateLichHenDTO với trạng thái và nhân viên (nếu có)
        var updateDto = new UpdateLichHenDTO
        {
            TrangThai = statusDto.TrangThai,
            MaNhanVien = maNhanVien
        };

        var result = await lichHenRepo.UpdateAsync(id, updateDto);
        if (result == null)
        {
            return BadRequest(new { Message = "Không thể cập nhật trạng thái" });
        }

        _logger.LogInformation("Successfully updated appointment ID: {AppointmentId} to status: {Status}", id, statusDto.TrangThai);
        return Ok(result);
    }
}