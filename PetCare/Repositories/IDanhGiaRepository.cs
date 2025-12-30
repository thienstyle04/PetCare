using PetCare.Models.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IDanhGiaRepository
{
    Task<List<DanhGia>> GetAllAsync(string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = true, int pageNumber = 1, int pageSize = 100);
    Task<DanhGia?> GetByIdAsync(int id);
    Task<DanhGia> AddAsync(DanhGia danhGia);
    Task<DanhGia?> UpdateAsync(int id, DanhGia danhGia);
    Task<DanhGia?> DeleteAsync(int id);
}
