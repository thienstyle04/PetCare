using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetCare.Data;
using PetCare.Models.DTO;
using PetCare.Repositories;
using System.Threading.Tasks;

namespace PetCare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class KhachhangController : ControllerBase
    {
        private readonly IKhachHangRepositories _khachhangRepositories;
        private readonly ILogger<KhachhangController> _logger;

        public KhachhangController(IKhachHangRepositories khachhangRepositories, ILogger<KhachhangController> logger)
        {
            _khachhangRepositories = khachhangRepositories;
            _logger = logger;
        }
        [HttpPost("addkhachhang")]
        
        public async Task<IActionResult> RegisterKhachHang([FromBody] RegisterKhachHangDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _khachhangRepositories.RegisterKhachHangAsync(dto);

                return Ok(new
                {
                    Message = "Đăng ký khách hàng thành công!",
                    UserId = result.user.MaNguoiDung,
                    CustomerId = result.customer.MaKhachHang
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
        [HttpGet("getallkhachhang")]
        public IActionResult GetAllKhachHang(
            [FromQuery] string? filteron,
            [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy,
            [FromQuery] bool isAscending = true, // isAscending có giá trị mặc định 
            [FromQuery] int pageNumber = 1, // pageNumber có giá trị mặc định 
            [FromQuery] int pageSize = 1000) // pageSize có giá trị mặc định 
        {
            _logger.LogInformation("Fetching all customers with Filter, Sort, Pagination.");
            var customers = _khachhangRepositories.GetAllKhachHang(
                filteron,
                filterQuery,
                sortBy,
                isAscending,
                pageNumber,
                pageSize);

            return Ok(customers);
        }

        [HttpGet("getkhachhangById/{maKhachHang:int}")]
        public IActionResult GetKhachHangById([FromRoute] int maKhachHang)
        {
            _logger.LogInformation("Fetching customer ID: {CustomerId}", maKhachHang);
            var customer = _khachhangRepositories.GetKhachHangById(maKhachHang);

            if (customer == null)
            {
                _logger.LogWarning("Customer ID {CustomerId} not found.", maKhachHang);
                return NotFound(new { Message = "Không tìm thấy khách hàng" });
            }

            return Ok(customer);
        }
        [HttpPut("UpdateKhachHang/{maKhachHang:int}")]
        public IActionResult UpdateKhachHang([FromRoute] int maKhachHang, [FromBody] KhachHangDTO updateDto)
        {
            _logger.LogWarning("Attempting to update customer ID: {CustomerId}", maKhachHang);

            var updatedCustomer = _khachhangRepositories.UpdateKhachHang(maKhachHang, updateDto);

            if (updatedCustomer == null)
            {
                _logger.LogError("Update failed: Customer ID {CustomerId} not found.", maKhachHang);
                return NotFound(new { Message = "Không tìm thấy khách hàng để cập nhật" });
            }

            _logger.LogInformation("Customer ID {CustomerId} successfully updated.", maKhachHang);
            return Ok(updatedCustomer);
        }
        [HttpDelete("DeleteKhachHangById/{maKhachHang:int}")]
        public async Task<IActionResult> DeleteKhachHang([FromRoute] int maKhachHang)
        {
            _logger.LogWarning("Attempting to delete customer ID: {CustomerId}", maKhachHang);

            try
            {
                var result = await _khachhangRepositories.DeleteKhachHangAsync(maKhachHang);

                if (!result)
                {
                    _logger.LogError("Deletion failed: Customer ID {CustomerId} not found in repository.", maKhachHang);
                    return NotFound(new { Message = "Không tìm thấy khách hàng" });
                }

                _logger.LogInformation("Customer ID {CustomerId} successfully deleted.", maKhachHang);
                return Ok(new { Message = "Đã xóa khách hàng thành công" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting customer ID: {CustomerId}", maKhachHang);
                return BadRequest(new { Message = "Lỗi khi xóa khách hàng", Error = ex.Message });
            }
        }
        [HttpGet("user/{maNguoiDung:int}")]
        public IActionResult GetKhachHangByMaNguoiDung([FromRoute] int maNguoiDung)
        {
            _logger.LogInformation("Fetching KhachHang by MaNguoiDung = {UserId}", maNguoiDung);

            var khachHang = _khachhangRepositories
                .GetAllKhachHang()
                .FirstOrDefault(k => k.MaNguoiDung == maNguoiDung);

            if (khachHang == null)
            {
                return NotFound(new { Message = "Không tìm thấy khách hàng tương ứng với người dùng này." });
            }

            return Ok(khachHang);
        }
    }
}