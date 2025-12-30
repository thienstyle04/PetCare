using PetCare.Models.Domain;

namespace PetCare.Repositories
{
    public interface IImageRepository
    {
        Task<Image> UploadAsync(Image image);
        Task<List<Image>> GetAllAsync();
        Task<Image?> GetByIdAsync(int id);
        Task<(byte[], string, string)?> DownloadAsync(int id);
        Task<Image?> DeleteAsync(int id);
    }
}
