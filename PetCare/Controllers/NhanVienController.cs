using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetCare.Models.DTO;
using PetCare.Repositories;

namespace PetCare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class NhanVienController : ControllerBase
    {
        private readonly INhanVienRepositories _nhanVienRepositories;
        private readonly ILogger<NhanVienController> _logger;

        public NhanVienController(INhanVienRepositories nhanVienRepositories, ILogger<NhanVienController> logger)
        {
            _nhanVienRepositories = nhanVienRepositories;
            _logger = logger;
        }

        // POST: api/NhanVien/addnhanvien
        [HttpPost("addnhanvien")]
        [Authorize(Roles = "QUANTRI")]
        public async Task<IActionResult> RegisterNhanVien([FromBody] RegisterNhanVienDTO dto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Registration validation failed for employee {Username}. Errors: {@Errors}",
                    dto.TenDangNhap, ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                _logger.LogInformation("Attempting to register employee: {Username}", dto.TenDangNhap);
                var result = await _nhanVienRepositories.RegisterNhanVienAsync(dto);

                _logger.LogInformation("Employee {Username} registered successfully with ID: {EmployeeId}",
                    dto.TenDangNhap, result.employee.MaNhanVien);

                return Ok(new
                {
                    Message = "Đăng ký nhân viên thành công!",
                    UserId = result.user.MaNguoiDung,
                    EmployeeId = result.employee.MaNhanVien
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering employee: {Username}", dto.TenDangNhap);
                return BadRequest(new { Message = ex.Message });
            }
        }

        // GET: api/NhanVien/getallnhanvien
        [HttpGet("getallnhanvien")]
        [Authorize(Roles = "QUANTRI")]
        public IActionResult GetAllNhanVien(
            [FromQuery] string? filteron,
            [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy,
            [FromQuery] bool isAscending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 1000)
        {
            _logger.LogInformation("Fetching all employees with Filter, Sort, Pagination.");

            var employees = _nhanVienRepositories.GetAllNhanVien(
                filteron,
                filterQuery,
                sortBy,
                isAscending,
                pageNumber,
                pageSize);

            _logger.LogDebug("Returned {Count} employees.", employees.Count);
            return Ok(employees);
        }

        // GET: api/NhanVien/getnhanvienById5
        [HttpGet("getnhanvienById/{maNhanVien:int}")]
        public IActionResult GetNhanVienById([FromRoute] int maNhanVien)
        {
            _logger.LogInformation("Fetching employee ID: {EmployeeId}", maNhanVien);
            var employee = _nhanVienRepositories.GetNhanVienById(maNhanVien);

            if (employee == null)
            {
                _logger.LogWarning("Employee ID {EmployeeId} not found.", maNhanVien);
                return NotFound(new { Message = "Không tìm thấy nhân viên" });
            }

            return Ok(employee);
        }

        // GET: api/NhanVien/user/{maNguoiDung}
        [HttpGet("user/{maNguoiDung:int}")]
        [Authorize(Roles = "NHANVIEN,QUANTRI")]
        public IActionResult GetByUserId([FromRoute] int maNguoiDung)
        {
            var employee = _nhanVienRepositories.GetNhanVienByUserId(maNguoiDung);
            if (employee == null)
            {
                return NotFound(new { Message = "Không tìm thấy nhân viên cho người dùng này" });
            }

            return Ok(employee);
        }

        [HttpGet("getEmployeeIdByUserId/{maNguoiDung:int}")]
        [Authorize(Roles = "NHANVIEN,QUANTRI")]
        public IActionResult GetEmployeeIdByUserId([FromRoute] int maNguoiDung)
        {
            var employee = _nhanVienRepositories.GetNhanVienByUserId(maNguoiDung);
            if (employee == null)
            {
                return NotFound(new { Message = "Không tìm thấy nhân viên cho người dùng này" });
            }

            return Ok(employee.MaNhanVien); // chỉ trả về MaNhanVien
        }
        // PUT: api/NhanVien/UpdateNhanVien5
        [HttpPut("UpdateNhanVien/{maNhanVien:int}")]
        [Authorize(Roles = "QUANTRI, NHANVIEN")]
        public IActionResult UpdateNhanVien([FromRoute] int maNhanVien, [FromBody] NhanVienDTO updateDto)
        {
            _logger.LogWarning("Attempting to update employee ID: {EmployeeId}", maNhanVien);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Update validation failed for employee ID: {EmployeeId}", maNhanVien);
                return BadRequest(ModelState);
            }

            var updatedEmployee = _nhanVienRepositories.UpdateNhanVien(maNhanVien, updateDto);

            if (updatedEmployee == null)
            {
                _logger.LogError("Update failed: Employee ID {EmployeeId} not found.", maNhanVien);
                return NotFound(new { Message = "Không tìm thấy nhân viên để cập nhật" });
            }

            _logger.LogInformation("Employee ID {EmployeeId} successfully updated.", maNhanVien);
            return Ok(updatedEmployee);
        }

        // DELETE: api/NhanVien/DeleteNhanVienById5
        [HttpDelete("DeleteNhanVienById/{maNhanVien:int}")]
        [Authorize(Roles = "QUANTRI")]
        public async Task<IActionResult> DeleteNhanVien([FromRoute] int maNhanVien)
        {
            _logger.LogWarning("Attempting to delete employee ID: {EmployeeId}", maNhanVien);

            try
            {
                var result = await _nhanVienRepositories.DeleteNhanVienAsync(maNhanVien);

                if (!result)
                {
                    _logger.LogError("Deletion failed: Employee ID {EmployeeId} not found in repository.", maNhanVien);
                    return NotFound(new { Message = "Không tìm thấy nhân viên" });
                }

                _logger.LogInformation("Employee ID {EmployeeId} successfully deleted.", maNhanVien);
                return Ok(new { Message = "Đã xóa nhân viên thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting employee ID: {EmployeeId}", maNhanVien);
                return BadRequest(new { Message = "Lỗi khi xóa nhân viên", Error = ex.Message });
            }
        }
    }
}