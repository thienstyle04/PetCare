using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // <-- THÊM Logger
using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class ThanhToanController : ControllerBase
{
    private readonly IThanhToanRepository thanhToanRepo;
    private readonly ILogger<ThanhToanController> _logger; // <-- Khai báo Logger

    public ThanhToanController(IThanhToanRepository repo, ILogger<ThanhToanController> logger)
    {
        thanhToanRepo = repo;
        _logger = logger; // Gán Logger
    }

    // GET: api/ThanhToan
    [HttpGet]
    [Route("allthanhtoan")]
    [Authorize(Roles = "QUANTRI")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? filterOn,
        [FromQuery] string? filterQuery,
        [FromQuery] string? sortBy,
        [FromQuery] bool isAscending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Request GET ALL Payments started. SortBy: {Sort}", sortBy);

        var result = await thanhToanRepo.GetAllAsync(
            filterOn,
            filterQuery,
            sortBy,
            isAscending,
            pageNumber,
            pageSize
        );

        _logger.LogDebug("Returned {Count} payment records.", result.Count);
        return Ok(result);
    }

    // GET: api/ThanhToan/{id}
    [HttpGet("Get_thanhtoan_id/{id}")]
    [Authorize(Roles = "QUANTRI")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Request GET Payment by ID: {PaymentId}", id);
        var result = await thanhToanRepo.GetByIdAsync(id);

        if (result == null)
        {
            _logger.LogWarning("Payment ID {PaymentId} not found.", id);
            return NotFound();
        }
        return Ok(result);
    }

    // POST: api/ThanhToan
    [HttpPost]
    [Route("themthanhtoan")]
    public async Task<IActionResult> Create([FromBody] ThanhToanRequestDTO requestDto)
    {
        _logger.LogWarning("Attempting to CREATE new payment for Appointment ID: {AppointmentId}", requestDto.MaLichHen);
        if (!ModelState.IsValid)
        {
            _logger.LogError("Validation failed for CREATE Payment.");
            return BadRequest(ModelState);
        }

        // Manually map from DTO to Domain Model
        var thanhToanDomainModel = new ThanhToan
        {
            MaLichHen = requestDto.MaLichHen,
            SoTien = requestDto.SoTien,
            PhuongThuc = requestDto.PhuongThuc,
            GhiChu = requestDto.GhiChu,
            TrangThai = "DaThanhToan", // Set a default status on creation
            NgayThanhToan = DateTime.Now
        };

        var result = await thanhToanRepo.AddAsync(thanhToanDomainModel);
        _logger.LogInformation("Payment created successfully. New ID: {PaymentId}", result.MaThanhToan);
        return CreatedAtAction(nameof(GetById), new { id = result.MaThanhToan }, result);
    }


    // PUT: api/ThanhToan/{id}
    [HttpPut("update_thanhtoan_id/{id}")]
    [Authorize(Roles = "QUANTRI")]
    public async Task<IActionResult> Update(int id, [FromBody] ThanhToanDTO updateDto)
    {
        _logger.LogWarning("Attempting to UPDATE payment ID: {PaymentId} to Status: {Status}", id, updateDto.TrangThai);

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await thanhToanRepo.UpdateAsync(id, updateDto);

        if (result == null)
        {
            _logger.LogError("Update failed: Payment ID {PaymentId} not found.", id);
            return NotFound();
        }
        _logger.LogInformation("Payment ID {PaymentId} updated successfully.", id);
        return Ok(result);
    }

    // GET: api/ThanhToan/khachhang/{maKhachHang}
    [HttpGet("khachhang/{maKhachHang}")]
    [Authorize(Roles = "KHACHHANG, QUANTRI")]
    public async Task<IActionResult> GetByKhachHang(int maKhachHang)
    {
        _logger.LogInformation("Fetching payments for customer ID: {CustomerId}", maKhachHang);
        var result = await thanhToanRepo.GetByKhachHangIdAsync(maKhachHang);
        _logger.LogInformation("Found {Count} payments for customer ID: {CustomerId}", result.Count, maKhachHang);
        
        // Map Domain Model sang DTO
        var dtoList = result.Select(t => new ThanhToanDTO
        {
            MaThanhToan = t.MaThanhToan,
            MaLichHen = t.MaLichHen,
            SoTien = t.SoTien,
            PhuongThuc = t.PhuongThuc,
            TrangThai = t.TrangThai,
            NgayThanhToan = t.NgayThanhToan,
            GhiChu = t.GhiChu
        }).ToList();
        
        _logger.LogInformation("Mapped to DTO. First payment SoTien: {SoTien}", dtoList.FirstOrDefault()?.SoTien);
        
        return Ok(dtoList);
    }

    // DELETE: api/ThanhToan/{id}
    [HttpDelete("delete_thanhtoan_id/{id}")]
    [Authorize(Roles = "QUANTRI")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogWarning("Attempting to DELETE payment ID: {PaymentId}", id);
        var result = await thanhToanRepo.DeleteAsync(id);

        if (result == null)
        {
            _logger.LogError("Deletion failed: Payment ID {PaymentId} not found.", id);
            return NotFound();
        }
        _logger.LogInformation("Payment ID {PaymentId} successfully deleted.", id);
        return Ok(result);
    }
}