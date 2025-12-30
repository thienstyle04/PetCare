using Microsoft.EntityFrameworkCore;
using PetCare.Data;
using PetCare.Models.Domain;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SQLDanhGiaRepository : IDanhGiaRepository
{
    private readonly AppDbContext dbContext;
    public SQLDanhGiaRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public async Task<List<DanhGia>> GetAllAsync(string? filterOn = null,
        string? filterQuery = null, string? sortBy = null, bool isAscending = true, int pageNumber = 1, int pageSize = 10)
    {
        // Include navigation properties for DTO mapping
        var reviewsQuery = dbContext.DanhGia
            .Include(d => d.KhachHang)
            .Include(d => d.LichHen!)
                .ThenInclude(l => l.DichVu)
            .AsQueryable();

        // 2. FILTERING
        if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
        {
            if (filterOn.Equals("diemso", StringComparison.OrdinalIgnoreCase) && int.TryParse(filterQuery, out int score))
            {
                reviewsQuery = reviewsQuery.Where(x => x.DiemSo == score);
            }
            else if (filterOn.Equals("binhluan", StringComparison.OrdinalIgnoreCase))
            {
                reviewsQuery = reviewsQuery.Where(x => x.BinhLuan != null && x.BinhLuan.Contains(filterQuery));
            }
        }

        // 3. SORTING
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            if (sortBy.Equals("diemso", StringComparison.OrdinalIgnoreCase))
            {
                reviewsQuery = isAscending
                    ? reviewsQuery.OrderBy(x => x.DiemSo)
                    : reviewsQuery.OrderByDescending(x => x.DiemSo);
            }
            else if (sortBy.Equals("ngaytao", StringComparison.OrdinalIgnoreCase))
            {
                reviewsQuery = isAscending
                    ? reviewsQuery.OrderBy(x => x.NgayTao)
                    : reviewsQuery.OrderByDescending(x => x.NgayTao);
            }
        }

        // 4. PAGINATION
        var skipResults = (pageNumber - 1) * pageSize;

        return await reviewsQuery
            .Skip(skipResults)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<DanhGia?> GetByIdAsync(int id)
    {
        return await dbContext.DanhGia
            .Include(d => d.KhachHang)
            .Include(d => d.LichHen!)
                .ThenInclude(l => l.DichVu)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.MaDanhGia == id);
    }

    public async Task<DanhGia> AddAsync(DanhGia danhGia)
    {
        await dbContext.DanhGia.AddAsync(danhGia);
        await dbContext.SaveChangesAsync();
        return danhGia;
    }

    public async Task<DanhGia?> UpdateAsync(int id, DanhGia danhGia)
    {
        var existing = await dbContext.DanhGia.FirstOrDefaultAsync(x => x.MaDanhGia == id);
        if (existing == null) return null;

        existing.DiemSo = danhGia.DiemSo;
        existing.BinhLuan = danhGia.BinhLuan;

        await dbContext.SaveChangesAsync();
        return existing;
    }

    public async Task<DanhGia?> DeleteAsync(int id)
    {
        var existing = await dbContext.DanhGia.FirstOrDefaultAsync(x => x.MaDanhGia == id);
        if (existing == null) return null;

        dbContext.DanhGia.Remove(existing);
        await dbContext.SaveChangesAsync();
        return existing;
    }
}
