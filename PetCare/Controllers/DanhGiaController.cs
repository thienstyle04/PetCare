using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class DanhGiaController : ControllerBase
{
    private readonly IDanhGiaRepository danhGiaRepo;
    private readonly ILogger<DanhGiaController> _logger;

    public DanhGiaController(IDanhGiaRepository repo, ILogger<DanhGiaController> logger)
    {
        danhGiaRepo = repo;
        _logger = logger;
    }

    // GET: api/DanhGia - GetAll
    [HttpGet]
    [Authorize(Roles = "NHANVIEN, QUANTRI")]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? filterOn,
        [FromQuery] string? filterQuery,
        [FromQuery] string? sortBy,
        [FromQuery] bool isAscending = true,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        _logger.LogInformation("Request GET ALL received. FilterOn: {FilterOn}, SortBy: {SortBy}", filterOn, sortBy);
        
        var result = await danhGiaRepo.GetAllAsync(
            filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize
        );
        
        // Map to DTO with customer names
        var dtoList = result.Select(r => new DanhGiaDTO
        {
            MaDanhGia = r.MaDanhGia,
            MaLichHen = r.MaLichHen,
            MaKhachHang = r.MaKhachHang,
            DiemSo = r.DiemSo,
            BinhLuan = r.BinhLuan,
            NgayTao = r.NgayTao,
            TenKhachHang = r.KhachHang != null ? r.KhachHang.HoTen : "N/A",
            TenDichVu = r.LichHen != null && r.LichHen.DichVu != null ? r.LichHen.DichVu.TenDichVu : "N/A"
        }).ToList();
        
        _logger.LogDebug("Successfully retrieved {Count} reviews.", dtoList.Count);
        _logger.LogInformation("Completed getting all reviews. Total count: {Count}", dtoList.Count);
        
        return Ok(dtoList);
    }

    // GET: api/DanhGia/{id} - GetById
    [HttpGet("{id}")]
    [Authorize(Roles = "NHANVIEN, QUANTRI")]
    public async Task<IActionResult> GetById(int id)
    {
        _logger.LogInformation("Request GET by ID: {Id}", id);
        var result = await danhGiaRepo.GetByIdAsync(id);

        if (result == null)
        {
            _logger.LogWarning("Review ID {Id} not found.", id);
            return NotFound(new { Message = $"Không tìm thấy đánh giá ID: {id}" });
        }

        // Map to DTO with customer and service names
        var dto = new DanhGiaDTO
        {
            MaDanhGia = result.MaDanhGia,
            MaLichHen = result.MaLichHen,
            MaKhachHang = result.MaKhachHang,
            DiemSo = result.DiemSo,
            BinhLuan = result.BinhLuan,
            NgayTao = result.NgayTao,
            TenKhachHang = result.KhachHang?.HoTen ?? "N/A",
            TenDichVu = result.LichHen?.DichVu?.TenDichVu ?? "N/A"
        };

        _logger.LogInformation("Successfully retrieved review ID: {Id}", id);
        return Ok(dto);
    }

    // GET: api/DanhGia/khachhang/{maKhachHang} - Get reviews by customer
    [HttpGet("khachhang/{maKhachHang}")]
    [Authorize(Roles = "KHACHHANG, NHANVIEN, QUANTRI")]
    public async Task<IActionResult> GetByKhachHang(int maKhachHang)
    {
        _logger.LogInformation("Fetching reviews for customer ID: {MaKhachHang}", maKhachHang);
        var result = await danhGiaRepo.GetAllAsync(
            filterOn: "MaKhachHang", 
            filterQuery: maKhachHang.ToString(), 
            sortBy: null, 
            isAscending: true, 
            pageNumber: 1, 
            pageSize: 1000
        );
        
        _logger.LogInformation("Found {Count} reviews for customer ID: {MaKhachHang}", result.Count, maKhachHang);
        return Ok(result);
    }

    // POST: api/DanhGia - Create
    [HttpPost]
    [Authorize(Roles = "KHACHHANG")]
    public async Task<IActionResult> Create([FromBody] DanhGia danhGia)
    {
        _logger.LogWarning("Attempting to CREATE new review. Score: {Score}", danhGia.DiemSo);

        var result = await danhGiaRepo.AddAsync(danhGia);

        _logger.LogInformation("Review created successfully. New ID: {ReviewId}", result.MaDanhGia);
        return CreatedAtAction(nameof(GetById), new { id = result.MaDanhGia }, result);
    }

    // PUT: api/DanhGia/{id} - Update
    [HttpPut("{id}")]
    [Authorize(Roles = "KHACHHANG")]
    public async Task<IActionResult> Update(int id, [FromBody] DanhGia danhGia)
    {
        _logger.LogWarning("Attempting to UPDATE review ID: {Id} to score {Score}", id, danhGia.DiemSo);

        var result = await danhGiaRepo.UpdateAsync(id, danhGia);

        if (result == null)
        {
            _logger.LogError("Update failed: Review ID {Id} not found in database.", id);
            return NotFound(new { Message = $"Không tìm thấy đánh giá ID: {id} để cập nhật." });
        }

        _logger.LogInformation("Review ID {Id} updated successfully.", id);
        return Ok(result);
    }

    // DELETE: api/DanhGia/{id} - Delete
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        _logger.LogWarning("Attempting to DELETE review ID: {Id}", id);

        var result = await danhGiaRepo.DeleteAsync(id);

        if (result == null)
        {
            _logger.LogError("Deletion failed: Review ID {Id} not found.", id);
            return NotFound(new { Message = $"Không tìm thấy đánh giá ID: {id} để xóa." });
        }

        _logger.LogInformation("Review ID {Id} successfully deleted.", id);
        return Ok(result);
    }
}