using PetCare.Models.Domain;
using PetCare.Models.DTO;

namespace PetCare.Repositories
{
    public interface INhanVienRepositories
    {
        Task<(NguoiDung user, NhanVien employee)> RegisterNhanVienAsync(RegisterNhanVienDTO dto);
        List<NhanVien> GetAllNhanVien(
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 1000);
        NhanVien? GetNhanVienById(int maNhanVien);
        NhanVien? UpdateNhanVien(int maNhanVien, NhanVienDTO updateDto);
        Task<bool> DeleteNhanVienAsync(int maNhanVien);
        NhanVien? GetNhanVienByUserId(int maNguoiDung);
        NhanVien? GetNhanVienByIdentityUserId(string identityUserId);
    }
}