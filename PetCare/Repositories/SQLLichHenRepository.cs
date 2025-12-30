using Microsoft.EntityFrameworkCore;
using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SQLLichHenRepository : ILichHenRepository
{
    private readonly AppDbContext dbContext;

    public SQLLichHenRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<LichHenDTO>> GetAllAsync(
        string? filterOn = null, string? filterQuery = null,
        string? sortBy = null, bool isAscending = true,
        int pageNumber = 1, int pageSize = 10)
    {
        var appointmentsQuery = dbContext.LichHen.AsQueryable();

        // Logic filtering và sorting của bạn giữ nguyên
        // ...

        var skipResults = (pageNumber - 1) * pageSize;

        return await appointmentsQuery
            .Skip(skipResults)
            .Take(pageSize)
            .Select(lh => new LichHenDTO
            {
                MaLichHen = lh.MaLichHen,
                MaKhachHang = lh.MaKhachHang,
                TenKhachHang = lh.KhachHang.HoTen,
                MaThuCung = lh.MaThuCung,
                TenThuCung = lh.ThuCung.TenThuCung,
                MaDichVu = lh.MaDichVu,
                TenDichVu = lh.DichVu.TenDichVu,
                MaNhanVien = lh.MaNhanVien,
                TenNhanVien = lh.NhanVien != null ? lh.NhanVien.HoTen : null,
                NgayGioHen = lh.NgayGioHen,
                TrangThai = lh.TrangThai,
                GhiChu = lh.GhiChu,
                TongTien = lh.TongTien,
                NgayTao = lh.NgayTao
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<LichHenDTO?> GetByIdAsync(int id)
    {
        return await dbContext.LichHen
            .Where(x => x.MaLichHen == id)
            .Select(lh => new LichHenDTO
            {
                MaLichHen = lh.MaLichHen,
                MaKhachHang = lh.MaKhachHang,
                TenKhachHang = lh.KhachHang.HoTen,
                MaThuCung = lh.MaThuCung,
                TenThuCung = lh.ThuCung.TenThuCung,
                MaDichVu = lh.MaDichVu,
                TenDichVu = lh.DichVu.TenDichVu,
                MaNhanVien = lh.MaNhanVien,
                TenNhanVien = lh.NhanVien != null ? lh.NhanVien.HoTen : null,
                NgayGioHen = lh.NgayGioHen,
                TrangThai = lh.TrangThai,
                GhiChu = lh.GhiChu,
                TongTien = lh.TongTien,
                NgayTao = lh.NgayTao
            })
            .FirstOrDefaultAsync();
    }

    public async Task<LichHen?> AddAsync(LichHenRequestDTO lichHenRequestDto)
    {
        // ✅ SỬA LỖI: Sửa 'MaDichVu' từ kiểu Nullable<int> thành int
        var dichVu = await dbContext.DichVu.FindAsync(lichHenRequestDto.MaDichVu);
        if (dichVu == null) return null;

        var lichHen = new LichHen
        {
            MaKhachHang = lichHenRequestDto.MaKhachHang,
            MaThuCung = lichHenRequestDto.MaThuCung,
            // ✅ SỬA LỖI: 'MaDichVu' không cần .Value vì nó là kiểu int
            MaDichVu = lichHenRequestDto.MaDichVu,
            MaNhanVien = lichHenRequestDto.MaNhanVien,
            // ✅ SỬA LỖI: DTO sử dụng 'NgayGioHen', không phải 'ThoiGian'
            NgayGioHen = lichHenRequestDto.NgayGioHen,
            TrangThai = lichHenRequestDto.TrangThai,
            GhiChu = lichHenRequestDto.GhiChu,
            TongTien = dichVu.Gia
        };

        await dbContext.LichHen.AddAsync(lichHen);
        await dbContext.SaveChangesAsync();
        return lichHen;
    }

    public async Task<LichHenDTO?> UpdateAsync(int id, UpdateLichHenDTO updateLichHenDto)
    {
        var existingLichHen = await dbContext.LichHen.FirstOrDefaultAsync(x => x.MaLichHen == id);
        if (existingLichHen == null) return null;

        // Chỉ cập nhật các trường không null
        if (updateLichHenDto.MaNhanVien.HasValue)
            existingLichHen.MaNhanVien = updateLichHenDto.MaNhanVien;
        
        if (updateLichHenDto.NgayGioHen.HasValue)
            existingLichHen.NgayGioHen = updateLichHenDto.NgayGioHen.Value;
        
        if (!string.IsNullOrEmpty(updateLichHenDto.TrangThai))
            existingLichHen.TrangThai = updateLichHenDto.TrangThai;
        
        if (updateLichHenDto.GhiChu != null)
            existingLichHen.GhiChu = updateLichHenDto.GhiChu;
        
        if (updateLichHenDto.TongTien.HasValue)
            existingLichHen.TongTien = updateLichHenDto.TongTien;

        await dbContext.SaveChangesAsync();
        return await GetByIdAsync(id);
    }

    public async Task<LichHen?> DeleteAsync(int id)
    {
        var existing = await dbContext.LichHen.FirstOrDefaultAsync(x => x.MaLichHen == id);
        if (existing == null) return null;

        dbContext.LichHen.Remove(existing);
        await dbContext.SaveChangesAsync();
        return existing;
    }
}