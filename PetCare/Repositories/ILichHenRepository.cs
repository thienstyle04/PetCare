using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface ILichHenRepository
{
    // ✅ SỬA LỖI: Đồng bộ kiểu trả về thành Task<List<LichHenDTO>>
    Task<List<LichHenDTO>> GetAllAsync(string? filterOn = null, string? filterQuery = null, string? sortBy = null, bool isAscending = true, int pageNumber = 1, int pageSize = 10);

    // ✅ SỬA LỖI: Đồng bộ kiểu trả về thành Task<LichHenDTO?>
    Task<LichHenDTO?> GetByIdAsync(int id);

    Task<LichHen?> DeleteAsync(int id);
    Task<LichHen?> AddAsync(LichHenRequestDTO lichHenRequestDto);
    Task<LichHenDTO?> UpdateAsync(int id, UpdateLichHenDTO updateLichHenDto);
}