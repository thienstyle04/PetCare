using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using PetCare.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PetCare.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ThuCungController : ControllerBase
    {
        private readonly IThuCungRepositories _thuCungRepositories;
        private readonly AppDbContext _dbcontext;
        private readonly IImageRepository _imageRepository;
        private readonly ILogger<ThuCungController> _logger;

        public ThuCungController(
            IThuCungRepositories thuCungRepositories,
            AppDbContext dbcontext,
            IImageRepository imageRepository,
            ILogger<ThuCungController> logger)
        {
            _thuCungRepositories = thuCungRepositories;
            _dbcontext = dbcontext;
            _imageRepository = imageRepository;
            _logger = logger;
        }
        [HttpGet("khachhang/{maKhachHang:int}")]
        [Authorize(Roles = "KHACHHANG, QUANTRI, NHANVIEN")]
        public IActionResult GetPetsByKhachHang([FromRoute] int maKhachHang)
        {
            _logger.LogInformation("Fetching pets for customer ID: {CustomerId}", maKhachHang);
            
            var pets = _dbcontext.ThuCung
                .Where(t => t.MaKhachHang == maKhachHang)
                .Select(t => new ThuCungDTO
                {
                    MaThuCung = t.MaThuCung,
                    MaKhachHang = t.MaKhachHang,
                    TenThuCung = t.TenThuCung,
                    Loai = t.Loai,
                    Giong = t.Giong,
                    GioiTinh = t.GioiTinh,
                    NgaySinh = t.NgaySinh,
                    NamSinh = t.NamSinh,
                    Tuoi = t.Tuoi,
                    CanNang = t.CanNang,
                    ChuThich = t.ChuThich
                })
                .ToList();

            _logger.LogInformation("Found {Count} pets for customer ID: {CustomerId}", pets.Count, maKhachHang);
            return Ok(pets);
        }

        [HttpPost("addthucung")]
        [Authorize(Roles = "KHACHHANG")]
        public IActionResult AddPet([FromBody] ThuCungDTO petDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Validation failed for Add Pet. Errors: {@Errors}", ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Attempting to add new pet: {PetName}", petDto.TenThuCung);
            var petDomainModel = _thuCungRepositories.AddPet(petDto);

            _logger.LogInformation("New pet added successfully with ID: {PetId}", petDomainModel.MaThuCung);
            return CreatedAtAction(nameof(GetPetById), new { maThuCung = petDomainModel.MaThuCung }, petDomainModel);
        }
        [HttpGet("getallthucung")]
        [Authorize(Roles = "KHACHHANG, QUANTRI, NHANVIEN")]
        public IActionResult GetAllPets(
            [FromQuery] string? filterOn, [FromQuery] string? filterQuery,
            [FromQuery] string? sortBy, [FromQuery] bool isAscending = true,
            [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            _logger.LogInformation("Fetching all pets with filterOn: {FilterOn}, query: {Query}", filterOn, filterQuery);
            var pets = _thuCungRepositories.GetAllPets(
                filterOn, filterQuery, sortBy, isAscending, pageNumber, pageSize);

            if (pets == null || !pets.Any())
            {
                return Ok(new List<ThuCungDTO>());
            }

            // ✅ Map Domain Model sang DTO để trả về đầy đủ thông tin
            var petDtos = pets.Select(t => new ThuCungDTO
            {
                MaThuCung = t.MaThuCung,
                MaKhachHang = t.MaKhachHang,
                TenThuCung = t.TenThuCung,
                Loai = t.Loai,
                Giong = t.Giong,
                GioiTinh = t.GioiTinh,
                NgaySinh = t.NgaySinh,
                NamSinh = t.NamSinh,
                Tuoi = t.Tuoi,
                CanNang = t.CanNang,
                ChuThich = t.ChuThich
            }).ToList();

            _logger.LogInformation("Returned {Count} pets for page {PageNumber}.", petDtos.Count, pageNumber);
            return Ok(petDtos);
        }
        [HttpGet("addthucungById/{maThuCung:int}")]
        [Authorize(Roles = "KHACHHANG, QUANTRI, NHANVIEN")]
        public IActionResult GetPetById([FromRoute] int maThuCung)
        {
            _logger.LogInformation("Attempting to get pet ID: {PetId}", maThuCung);
            var pet = _thuCungRepositories.GetPetById(maThuCung);

            if (pet == null)
            {
                _logger.LogWarning("Pet ID {PetId} not found.", maThuCung);
                return NotFound(new { Message = $"Không tìm thấy thú cưng mã {maThuCung}." });
            }

            // ✅ Map Domain Model sang DTO
            var petDto = new ThuCungDTO
            {
                MaThuCung = pet.MaThuCung,
                MaKhachHang = pet.MaKhachHang,
                TenThuCung = pet.TenThuCung,
                Loai = pet.Loai,
                Giong = pet.Giong,
                GioiTinh = pet.GioiTinh,
                NgaySinh = pet.NgaySinh,
                NamSinh = pet.NamSinh,
                Tuoi = pet.Tuoi,
                CanNang = pet.CanNang,
                ChuThich = pet.ChuThich
            };

            _logger.LogInformation("Pet ID {PetId} retrieved successfully.", maThuCung);
            return Ok(petDto);
        }
        [HttpPut("updatepet/{maThuCung:int}")]
        [Authorize(Roles = "KHACHHANG, QUANTRI")]
        public IActionResult UpdatePet([FromRoute] int maThuCung, [FromBody] ThuCungDTO petDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogError("Validation failed for Update Pet ID {PetId}. Errors: {@Errors}", maThuCung, ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }
            _logger.LogWarning("Attempting to update pet ID: {PetId}", maThuCung);
            var updatedPet = _thuCungRepositories.UpdatePet(maThuCung, petDto);

            if (updatedPet == null)
            {
                _logger.LogError("Update failed: Pet ID {PetId} not found.", maThuCung);
                return NotFound(new { Message = $"Không tìm thấy thú cưng mã {maThuCung} để cập nhật." });
            }

            _logger.LogInformation("Pet ID {PetId} updated successfully.", maThuCung);
            return Ok(updatedPet);
        }
        [HttpDelete("DeleteByID/{maThuCung:int}")]
        [Authorize(Roles = "KHACHHANG, QUANTRI")]
        public IActionResult DeletePet([FromRoute] int maThuCung)
        {
            _logger.LogWarning("Attempting to delete pet ID: {PetId}", maThuCung);

            try
            {
                var deletedPet = _thuCungRepositories.DeletePet(maThuCung);

                if (deletedPet == null)
                {
                    _logger.LogError("Deletion failed: Pet ID {PetId} not found in database.", maThuCung);
                    return NotFound(new { Message = $"Không tìm thấy thú cưng mã {maThuCung} để xóa." });
                }

                _logger.LogInformation("Pet ID {PetId} successfully deleted.", maThuCung);
                return Ok(new { Message = "Xóa thú cưng thành công", Data = deletedPet });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Cannot delete pet ID {PetId} due to existing references.", maThuCung);
                return BadRequest(new { Message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting pet ID {PetId}", maThuCung);
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi xóa thú cưng" });
            }
        }
        [HttpPost("uploadpetphoto/{maThuCung}/photo")]
        [Authorize(Roles = "KHACHHANG, QUANTRI")] 
        public async Task<IActionResult> UploadPetPhoto(int maThuCung, [FromForm] ImageUploadRequestDTO request)
        {
            _logger.LogInformation("Starting photo upload for pet ID: {PetId}", maThuCung);
            var pet = await _dbcontext.ThuCung.FindAsync(maThuCung);
            if (pet == null)
            {
                _logger.LogError("Pet ID {PetId} not found", maThuCung);
                return NotFound(new { Message = $"Không tìm thấy thú cưng mã {maThuCung}" });
            }
            if (request.File == null || request.File.Length == 0)
            {
                _logger.LogError("Upload rejected: File is null or empty.");
                return BadRequest(new { Message = "File không hợp lệ hoặc rỗng." });
            }

            var allowedExt = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };
            var ext = System.IO.Path.GetExtension(request.File.FileName).ToLower();
            if (!allowedExt.Contains(ext))
            {
                _logger.LogError("Invalid file extension: {Ext}", ext);
                return BadRequest(new { Message = "Định dạng file không được hỗ trợ." });
            }
            if (request.File.Length > 10 * 1024 * 1024)
            {
                _logger.LogError("File too large: {Size}", request.File.Length);
                return BadRequest(new { Message = "Kích thước file tối đa 10MB." });
            }
            request.MaThuCung = maThuCung;
            var imageDomainModel = new Image
            {
                File = request.File,
                FileExtension = ext,
                FileSizeInBytes = request.File.Length,
                FileName = string.IsNullOrWhiteSpace(request.FileName)
                    ? $"pet_{maThuCung}_{DateTime.Now:yyyyMMdd_HHmmss}"
                    : request.FileName,
                FileDescription = request.FileDescription ?? $"Ảnh thú cưng {maThuCung}",
                MaThuCung = maThuCung
            };
            var uploadedImage = await _imageRepository.UploadAsync(imageDomainModel);

            _logger.LogInformation("Photo uploaded successfully for pet ID: {PetId}", maThuCung);

            return Ok(new
            {
                Message = "Upload ảnh thú cưng thành công.",
                ImageId = uploadedImage.Id,
                FilePath = uploadedImage.FilePath
            });
        }
    }
}