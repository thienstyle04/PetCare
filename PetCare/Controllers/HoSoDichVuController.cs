using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetCare.Data;
using PetCare.Models.DTO;
using PetCare.Repositories;

namespace PetCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class HoSoDichVuController : ControllerBase
    {
        private readonly IHoSoDichVuRepositories _hoSoRepo;
        private readonly AppDbContext _dbcontext;
        private readonly ILogger<HoSoDichVuController> _logger;

        public HoSoDichVuController(IHoSoDichVuRepositories hoSoRepo, AppDbContext dbcontext, ILogger<HoSoDichVuController> logger)
        {
            _hoSoRepo = hoSoRepo;
            _dbcontext = dbcontext;
            _logger = logger;
        }

        [HttpGet("allhosodichvu")]
        [Authorize(Roles = "NHANVIEN, QUANTRI, KHACHHANG")]
        public IActionResult GetAllHoSoDichVu([FromQuery] string? filterOn,
            [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy,
            [FromQuery] bool isAscending = true,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 100)
        {
            var result = _hoSoRepo.AllHoSoDichVu(filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("gethosodichvu_id/{MaHoSo:int}")]
        [Authorize(Roles = "NHANVIEN, QUANTRI")]
        public IActionResult GetHoSoDichVuById(int MaHoSo)
        {
            var result = _hoSoRepo.GetHoSoDichVuById(MaHoSo);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpPost("themhosodichvu")]
        [Authorize(Roles = "NHANVIEN, QUANTRI")]
        public IActionResult AddHoSoDichVu([FromBody] HoSoDichVuDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var added = _hoSoRepo.AddHoSoDichVu(dto);
            return CreatedAtAction(nameof(GetHoSoDichVuById), new { MaHoSo = added.MaHoSo }, added);
        }

        [HttpPut("updatehosodichvu_id/{MaHoSo:int}")]
        [Authorize(Roles = "NHANVIEN, QUANTRI")]
        public IActionResult UpdateHoSoDichVu(int MaHoSo, [FromBody] HoSoDichVuDTO dto)
        {
            var updated = _hoSoRepo.UpdateHoSoDichVu(MaHoSo, dto);
            if (updated == null) return NotFound();
            return Ok(updated);
        }

        [HttpDelete("deletehosodichvu_id/{MaHoSo:int}")]
        [Authorize(Roles = "NHANVIEN, QUANTRI")]
        public IActionResult DeleteHoSoDichVu(int MaHoSo)
        {
            var deleted = _hoSoRepo.DeleteHoSoDichVu(MaHoSo);
            if (deleted == null) return NotFound();
            return Ok(deleted);
        }
    }
}
