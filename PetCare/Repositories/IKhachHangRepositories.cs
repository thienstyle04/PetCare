using PetCare.Models.Domain;
using PetCare.Models.DTO;

namespace PetCare.Repositories
{
    public interface IKhachHangRepositories
    {
        List<KhachHang> GetAllKhachHang(
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 1000);
        KhachHang? GetKhachHangById(int maKhachHang);
        KhachHang? UpdateKhachHang(int maKhachHang, KhachHangDTO updateDto);
        Task<bool> DeleteKhachHangAsync(int maKhachHang);
        Task<(NguoiDung user, KhachHang customer)> RegisterKhachHangAsync(RegisterKhachHangDTO dto);
    }
}