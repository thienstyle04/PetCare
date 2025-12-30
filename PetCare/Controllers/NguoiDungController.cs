using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // <-- THÊM Logger
using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using PetCare.Repositories;
using System.Linq;
using System.Threading.Tasks;

namespace PetCare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "QUANTRI")]
    public class NguoiDungController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenRepositories _tokenRepositories;
        private readonly AppDbContext _dbcontext;
        private readonly ILogger<NguoiDungController> _logger; // <-- THÊM Logger

        public NguoiDungController(
            UserManager<IdentityUser> userManager,
            ITokenRepositories tokenRepositories,
            AppDbContext dbContext,
            ILogger<NguoiDungController> logger) // <-- Inject Logger
        {
            _userManager = userManager;
            _tokenRepositories = tokenRepositories;
            _dbcontext = dbContext;
            _logger = logger; // Gán Logger
        }

        // post API đăng ký
        
       

        // LẤY TẤT CẢ NGƯỜI DÙNG (CẬP NHẬT FSP)
        [HttpGet]
        [Route("GetALLNguoiDung")]
        
        public IActionResult GetAllNguoiDung(
            [FromQuery] string? filterOn,
            [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy,
            [FromQuery] bool isAscending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            _logger.LogInformation("Request for GetAllNguoiDung received.");

            // Hàm này cần được cập nhật trong ITokenRepositories và lớp triển khai
            var result = _tokenRepositories.Allnguoidung(
                filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize
            );

            _logger.LogDebug("Returned {Count} users.", result.Count);
            return Ok(result);
        }

        // Lấy người dùng theo ID
        [HttpGet("Get-ND-By-ID:{id}")]
        public IActionResult GetNguoiDungById(int id)
        {
            _logger.LogInformation("Request GetNguoiDungById: {Id}", id);
            var result = _tokenRepositories.GetNguoiDungById(id);

            if (result == null)
            {
                _logger.LogWarning("User ID {Id} not found.", id);
                return NotFound(new { message = "Không tìm thấy người dùng" });
            }
            return Ok(result);
        }

        // Cập nhật người dùng
        [HttpPut("Update-ND-By-ID: {id}")]
        public IActionResult UpdateNguoiDung(int id, [FromBody] NguoiDungDTO nguoiDung)
        {
            _logger.LogWarning("Attempting to update user ID: {Id}", id);

            var result = _tokenRepositories.updateNguoiDung(id, nguoiDung);

            if (result == null)
            {
                _logger.LogError("Update failed: User ID {Id} not found.", id);
                return NotFound(new { message = "Không tìm thấy người dùng để cập nhật" });
            }
            _logger.LogInformation("User ID {Id} updated successfully.", id);
            return Ok(result);
        }

        // Xoá người dùng
        [HttpDelete("Dele-ND-By-ID:{id}")]
        public async Task<IActionResult> DeleteNguoiDung(int id)
        {
            _logger.LogWarning("Attempting to delete user ID: {Id}", id);

            try
            {
                var result = await _tokenRepositories.DeleteNguoiDungAsync(id);

                if (result == null)
                {
                    _logger.LogError("Deletion failed: User ID {Id} not found.", id);
                    return NotFound(new { message = "Không tìm thấy người dùng để xoá" });
                }

                _logger.LogInformation("User ID {Id} successfully deleted.", id);
                return Ok(new { message = "Xoá người dùng và tài khoản thành công", data = result });
            }
            catch (Exception ex)
            {
                _logger.LogError("Error deleting user ID {Id}: {Error}", id, ex.Message);
                return BadRequest(new { message = "Lỗi khi xóa người dùng", error = ex.Message });
            }
        }
        [HttpGet("GetNhanVien")]
        public IActionResult GetNhanVien()
        {
            var nhanVienList = _dbcontext.NguoiDung
                .Where(u => u.VaiTro == "NhanVien")
                .Select(u => new
                {
                    u.MaNguoiDung,
                    u.TenDangNhap,
                    u.Email,
                    u.VaiTro,
                    u.TrangThai,
                    u.NgayTao,
                    u.NgayCapNhat
                })
                .ToList();

            return Ok(nhanVienList);
        }
    }
}