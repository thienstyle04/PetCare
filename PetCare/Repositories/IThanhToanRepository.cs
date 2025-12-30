using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IThanhToanRepository
{
    Task<List<ThanhToan>> GetAllAsync(string? filterOn = null,
        string? filterQuery = null,
        string? sortBy = null,
        bool isAscending = true,
        int pageNumber = 1,
        int pageSize = 10);
    Task<ThanhToan> GetByIdAsync(int id);
    Task<List<ThanhToan>> GetByKhachHangIdAsync(int maKhachHang);
    Task<ThanhToan> AddAsync(ThanhToan thanhToan);
    Task<ThanhToan?> UpdateAsync(int id, ThanhToanDTO thanhToanDto);
    Task<ThanhToan> DeleteAsync(int id);
}
