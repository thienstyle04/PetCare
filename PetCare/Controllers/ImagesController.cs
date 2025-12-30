using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using PetCare.Repositories;
using System.Linq;

namespace PetCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository _imageRepository;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(IImageRepository imageRepository, ILogger<ImagesController> logger)
        {
            _imageRepository = imageRepository;
            _logger = logger;
        }

        // POST: api/Images/Upload
        [HttpPost]
        [Route("Upload")]
        [Authorize(Roles = "KHACHHANG, NHANVIEN, QUANTRI")]
        public async Task<IActionResult> Upload([FromForm] ImageUploadRequestDTO request)
        {
            _logger.LogInformation("Starting image upload for file: {FileName}", request.FileName);

            // Validate file upload
            if (!ValidateFileUpload(request))
            {
                _logger.LogError("File validation failed for: {FileName}", request.FileName);
                return BadRequest(ModelState);
            }

            // Convert DTO to Domain model
            var imageDomainModel = new Image
            {
                File = request.File,
                FileExtension = Path.GetExtension(request.File.FileName),
                FileSizeInBytes = request.File.Length,
                FileName = request.FileName,
                FileDescription = request.FileDescription,
                MaThuCung = request.MaThuCung
            };

            // Use repository to upload image
            var uploadedImage = await _imageRepository.UploadAsync(imageDomainModel);

            _logger.LogInformation("Image uploaded successfully with ID: {ImageId}", uploadedImage.Id);

            return Ok(new ImageDTO
            {
                Id = uploadedImage.Id,
                FileName = uploadedImage.FileName,
                FileDescription = uploadedImage.FileDescription,
                FileExtension = uploadedImage.FileExtension,
                FileSizeInBytes = uploadedImage.FileSizeInBytes,
                FilePath = uploadedImage.FilePath,
                UploadedAt = uploadedImage.UploadedAt,
                MaThuCung = uploadedImage.MaThuCung
            });
        }

        // GET: api/Images
        [HttpGet]
        [Route("allimages")]
        public async Task<IActionResult> GetAllImages()
        {
            _logger.LogInformation("Request to get all images");

            var images = await _imageRepository.GetAllAsync();

            var imageDTOs = images.Select(img => new ImageDTO
            {
                Id = img.Id,
                FileName = img.FileName,
                FileDescription = img.FileDescription,
                FileExtension = img.FileExtension,
                FileSizeInBytes = img.FileSizeInBytes,
                FilePath = img.FilePath,
                UploadedAt = img.UploadedAt,
                MaThuCung = img.MaThuCung
            }).ToList();

            return Ok(imageDTOs);
        }

        // GET: api/Images/{id}
        [HttpGet("Get_image_id/{id:int}")]
        public async Task<IActionResult> GetImageById(int id)
        {
            _logger.LogInformation("Request to get image by ID: {ImageId}", id);

            var image = await _imageRepository.GetByIdAsync(id);

            if (image == null)
            {
                _logger.LogWarning("Image with ID {ImageId} not found", id);
                return NotFound();
            }

            var imageDTO = new ImageDTO
            {
                Id = image.Id,
                FileName = image.FileName,
                FileDescription = image.FileDescription,
                FileExtension = image.FileExtension,
                FileSizeInBytes = image.FileSizeInBytes,
                FilePath = image.FilePath,
                UploadedAt = image.UploadedAt,
                MaThuCung = image.MaThuCung
            };

            return Ok(imageDTO);
        }

        // GET: api/Images/Download/{id}
        [HttpGet]
        [Route("Download/{id:int}")]
        public async Task<IActionResult> DownloadImage(int id)
        {
            _logger.LogInformation("Request to download image with ID: {ImageId}", id);

            var result = await _imageRepository.DownloadAsync(id);

            if (result == null)
            {
                _logger.LogWarning("Download failed: Image with ID {ImageId} not found", id);
                return NotFound();
            }

            return File(result.Value.Item1, result.Value.Item2, result.Value.Item3);
        }

        // DELETE: api/Images/{id}
        [HttpDelete("delete_image_id/{id:int}")]
        public async Task<IActionResult> DeleteImage(int id)
        {
            _logger.LogWarning("Attempting to delete image with ID: {ImageId}", id);

            var deletedImage = await _imageRepository.DeleteAsync(id);

            if (deletedImage == null)
            {
                _logger.LogError("Delete failed: Image with ID {ImageId} not found", id);
                return NotFound();
            }

            _logger.LogInformation("Image with ID {ImageId} deleted successfully", id);
            return Ok(new { Message = "Xóa hình ảnh thành công" });
        }

        #region Private Methods

        private bool ValidateFileUpload(ImageUploadRequestDTO request)
        {
            var allowedExtensions = new string[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp" };

            if (!allowedExtensions.Contains(Path.GetExtension(request.File.FileName).ToLower()))
            {
                ModelState.AddModelError("file", "Định dạng file không được hỗ trợ. Chỉ chấp nhận: jpg, jpeg, png, gif, bmp");
            }

            if (request.File.Length > 10 * 1024 * 1024) // 10MB
            {
                ModelState.AddModelError("file", "Kích thước file quá lớn. Tối đa 10MB");
            }

            if (request.File.Length == 0)
            {
                ModelState.AddModelError("file", "File không được để trống");
            }

            return ModelState.ErrorCount == 0;
        }

        #endregion
    }
}
