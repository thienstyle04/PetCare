using Microsoft.EntityFrameworkCore;
using PetCare.Data;
using PetCare.Models.Domain;

namespace PetCare.Repositories
{
    public class LocalImageRepository : IImageRepository
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly AppDbContext _dbContext;

        public LocalImageRepository(IWebHostEnvironment webHostEnvironment,
            IHttpContextAccessor httpContextAccessor,
            AppDbContext dbContext)
        {
            _webHostEnvironment = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
        }

        public async Task<Image> UploadAsync(Image image)
        {
            // ✅ TẠO THƯ MỤC IMAGES NẾU CHƯA TỒN TẠI
            var imagesDirectory = Path.Combine(_webHostEnvironment.ContentRootPath, "Images");
            if (!Directory.Exists(imagesDirectory))
            {
                Directory.CreateDirectory(imagesDirectory);
            }

            // Tạo đường dẫn file local
            var localFilePath = Path.Combine(imagesDirectory, $"{image.FileName}{image.FileExtension}");

            // Upload image to local path
            using var stream = new FileStream(localFilePath, FileMode.Create);
            await image.File.CopyToAsync(stream);

            // Tạo URL file path
            var urlFilePath = $"{_httpContextAccessor.HttpContext.Request.Scheme}://" +
                              $"{_httpContextAccessor.HttpContext.Request.Host}" +
                              $"{_httpContextAccessor.HttpContext.Request.PathBase}/Images/" +
                              $"{image.FileName}{image.FileExtension}";

            image.FilePath = urlFilePath;

            // Add image to database
            await _dbContext.Images.AddAsync(image);
            await _dbContext.SaveChangesAsync();

            return image;
        }

        public async Task<List<Image>> GetAllAsync()
        {
            return await _dbContext.Images.ToListAsync();
        }

        public async Task<Image?> GetByIdAsync(int id)
        {
            return await _dbContext.Images
                .Include(i => i.ThuCung)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<(byte[], string, string)?> DownloadAsync(int id)
        {
            try
            {
                var image = await _dbContext.Images
                    .Where(x => x.Id == id)
                    .FirstOrDefaultAsync();

                if (image == null) return null;

                var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Images",
                    $"{image.FileName}{image.FileExtension}");

                if (!File.Exists(path)) return null;

                var bytes = await File.ReadAllBytesAsync(path);
                var fileName = $"{image.FileName}{image.FileExtension}";

                return (bytes, "application/octet-stream", fileName);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<Image?> DeleteAsync(int id)
        {
            var image = await _dbContext.Images.FirstOrDefaultAsync(x => x.Id == id);

            if (image == null) return null;

            // Delete file from local storage
            var path = Path.Combine(_webHostEnvironment.ContentRootPath, "Images",
                $"{image.FileName}{image.FileExtension}");

            if (File.Exists(path))
            {
                File.Delete(path);
            }

            _dbContext.Images.Remove(image);
            await _dbContext.SaveChangesAsync();

            return image;
        }
    }
}
