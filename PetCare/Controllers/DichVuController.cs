using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging; // <-- THÊM: Cần thiết cho ILogger
using PetCare.Data;
using PetCare.Models.DTO;
using PetCare.Repositories;
using System.Linq; // Cần thiết cho Any()

namespace PetCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class DichVuController : ControllerBase
    {
        private readonly IDichVuRepositories _dichVuRepositories;
        private readonly AppDbContext _dbcontext;
        private readonly ILogger<DichVuController> _logger; // <-- THÊM: Khai báo Logger

        public DichVuController(IDichVuRepositories dichVuRepositories, AppDbContext dbcontext, ILogger<DichVuController> logger)
        {
            _dichVuRepositories = dichVuRepositories;
            _dbcontext = dbcontext;
            _logger = logger; // <-- Gán Logger
        }

        // GET: api/DichVu/alldichvu (GetAll with FSP)
        [HttpGet("alldichvu")]
        [AllowAnonymous] // Cho phép truy cập công khai cho trang chủ
        public IActionResult GetAllDichVu(
            [FromQuery] string? filterOn,
            [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy,
            [FromQuery] bool isAscending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            _logger.LogInformation("Request started for GetAllDichVu. Filter: {FilterOn}/{FilterQuery}", filterOn, filterQuery); // <-- Log bắt đầu

            var result = _dichVuRepositories.AllDichVu(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);

            _logger.LogDebug("Successfully returned {Count} services.", result.Count); // <-- Log thành công
            return Ok(result);
        }

        // GET: api/DichVu/5
        [HttpGet("Get_dichVu_ID/{MaDichVu:int}")]
        [Authorize(Roles = "NHANVIEN, QUANTRI")]
        public IActionResult GetDichVuById(int MaDichVu)
        {
            _logger.LogInformation("Request for service ID: {ServiceId}", MaDichVu); // <-- Log ID
            var result = _dichVuRepositories.GetDichVuById(MaDichVu);

            if (result == null)
            {
                _logger.LogWarning("Service ID {ServiceId} not found.", MaDichVu); // <-- Log cảnh báo
                return NotFound();
            }
            return Ok(result);
        }

        // POST: api/DichVu/themdichvu
        [HttpPost("themdichvu")]
        [Authorize(Roles = "QUANTRI")]
        public IActionResult AddDichVu([FromBody] DichVuDTO dichVu)
        {
            _logger.LogWarning("Attempting to add new service: {ServiceName}", dichVu.TenDichVu); // <-- Log cảnh báo trước thao tác

            if (!ValidateAddDichVu(dichVu))
            {
                _logger.LogError("Validation failed for AddDichVu. Errors: {@Errors}", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)); // <-- Log lỗi chi tiết
                return BadRequest(ModelState);
            }

            var dichvuAdd = _dichVuRepositories.addichvu(dichVu);

            _logger.LogInformation("Service added successfully. ID: {ServiceId}", dichvuAdd.MaDichVu); // <-- Log thành công
            return CreatedAtAction(nameof(GetDichVuById), new { MaDichVu = dichvuAdd.MaDichVu }, dichvuAdd);
        }

        // PUT: api/DichVu/5
        [HttpPut("update_dichvu_ID/{MaDichVu:int}")]
        [Authorize(Roles = "QUANTRI")]
        public IActionResult UpdateDichVu(int MaDichVu, [FromBody] DichVuDTO dichVu)
        {
            _logger.LogWarning("Attempting to update service ID: {ServiceId}", MaDichVu);
            var updatedDichVu = _dichVuRepositories.updatedichvu(MaDichVu, dichVu);

            if (updatedDichVu == null)
            {
                _logger.LogError("Update failed: Service ID {ServiceId} not found.", MaDichVu);
                return NotFound();
            }

            _logger.LogInformation("Service ID {ServiceId} updated successfully.", MaDichVu);
            return Ok(updatedDichVu);
        }

        // DELETE: api/DichVu/5
        [HttpDelete("delete_dichvu_id/{MaDichVu:int}")]
        [Authorize(Roles = "QUANTRI")]
        public IActionResult DeleteDichVu(int MaDichVu)
        {
            _logger.LogWarning("Attempting to delete service ID: {ServiceId}", MaDichVu);
            
            try
            {
                var deletedDichVu = _dichVuRepositories.deletedichvu(MaDichVu);

                if (deletedDichVu == null)
                {
                    _logger.LogError("Deletion failed: Service ID {ServiceId} not found.", MaDichVu);
                    return NotFound(new { Message = $"Không tìm thấy dịch vụ với mã {MaDichVu}" });
                }

                _logger.LogInformation("Service ID {ServiceId} successfully deleted.", MaDichVu);
                return Ok(new { Message = "Xóa dịch vụ thành công", Data = deletedDichVu });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Cannot delete service ID {ServiceId} due to existing references.", MaDichVu);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service ID {ServiceId}", MaDichVu);
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi xóa dịch vụ" });
            }
        }

        // ValidateAddDichVu giữ nguyên
        private bool ValidateAddDichVu(DichVuDTO dto)
        {
            // ... (Logic Validation giữ nguyên) ...

            if (dto == null)
            {
                ModelState.AddModelError(nameof(dto), "Vui lòng nhập dữ liệu dịch vụ");
                return false;
            }
            // ... (Các checks khác) ...

            if (_dichVuRepositories.AllDichVu(null, null, null, true, 1, 1000).Any(n => n.TenDichVu.ToLower() == dto.TenDichVu.ToLower()))
            {
                ModelState.AddModelError(nameof(dto.TenDichVu), "Tên dịch vụ đã tồn tại");
            }

            return ModelState.ErrorCount == 0;
        }
    }
}