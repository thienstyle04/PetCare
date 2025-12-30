using Microsoft.EntityFrameworkCore;
using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class SQLThanhToanRepository : IThanhToanRepository
{
    private readonly AppDbContext dbContext;

    public SQLThanhToanRepository(AppDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    // CẬP NHẬT: Triển khai Filter, Sort, Pagination
    public async Task<List<ThanhToan>> GetAllAsync(
        string? filterOn = null,
        string? filterQuery = null,
        string? sortBy = null,
        bool isAscending = true,
        int pageNumber = 1,
        int pageSize = 10)
    {
        // 1. Tạo IQueryable ban đầu và Include Navigation Properties
        var paymentsQuery = dbContext.ThanhToan
            // .Include(t => t.LichHen) // Giả định có Navigation Property LichHen
            .AsQueryable();

        // 2. FILTERING
        if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
        {
            if (filterOn.Equals("trangthai", StringComparison.OrdinalIgnoreCase))
            {
                // Lọc theo trạng thái (ví dụ: DaThanhToan, ChoThanhToan)
                paymentsQuery = paymentsQuery.Where(x => x.TrangThai.Contains(filterQuery));
            }
            else if (filterOn.Equals("phuongthuc", StringComparison.OrdinalIgnoreCase))
            {
                // Lọc theo phương thức thanh toán (ví dụ: TienMat, ChuyenKhoan)
                paymentsQuery = paymentsQuery.Where(x => x.PhuongThuc != null && x.PhuongThuc.Contains(filterQuery));
            }
        }

        // 3. SORTING
        if (!string.IsNullOrWhiteSpace(sortBy))
        {
            if (sortBy.Equals("sotien", StringComparison.OrdinalIgnoreCase))
            {
                // Sắp xếp theo số tiền
                paymentsQuery = isAscending
                    ? paymentsQuery.OrderBy(x => x.SoTien)
                    : paymentsQuery.OrderByDescending(x => x.SoTien);
            }
            else if (sortBy.Equals("ngaythanhtoan", StringComparison.OrdinalIgnoreCase))
            {
                // Sắp xếp theo ngày thanh toán
                paymentsQuery = isAscending
                    ? paymentsQuery.OrderBy(x => x.NgayThanhToan)
                    : paymentsQuery.OrderByDescending(x => x.NgayThanhToan);
            }
        }

        // 4. PAGINATION
        var skipResults = (pageNumber - 1) * pageSize;

        return await paymentsQuery
            .Skip(skipResults)
            .Take(pageSize)
            .ToListAsync();
    }

    // GET BY ID
    public async Task<ThanhToan> GetByIdAsync(int id)
    {
        return await dbContext.ThanhToan.FirstOrDefaultAsync(x => x.MaThanhToan == id);
    }

    // GET BY KHACHHANG ID
    public async Task<List<ThanhToan>> GetByKhachHangIdAsync(int maKhachHang)
    {
        return await dbContext.ThanhToan
            .Include(t => t.LichHen)
                .ThenInclude(lh => lh.DichVu)
            .Include(t => t.LichHen)
                .ThenInclude(lh => lh.ThuCung)
            .Where(t => t.LichHen.MaKhachHang == maKhachHang)
            .OrderByDescending(t => t.NgayThanhToan)
            .ToListAsync();
    }

    // ADD
    public async Task<ThanhToan> AddAsync(ThanhToan thanhToan)
    {
        await dbContext.ThanhToan.AddAsync(thanhToan);
        await dbContext.SaveChangesAsync();
        return thanhToan;
    }

    // UPDATE
    public async Task<ThanhToan?> UpdateAsync(int id, ThanhToanDTO thanhToanDto)
    {
        var existingThanhToan = await dbContext.ThanhToan.FirstOrDefaultAsync(x => x.MaThanhToan == id);
        if (existingThanhToan == null)
        {
            return null;
        }

        // Cập nhật các trường từ DTO
        existingThanhToan.TrangThai = thanhToanDto.TrangThai;
        existingThanhToan.GhiChu = thanhToanDto.GhiChu;

        // Nếu thanh toán thành công, cập nhật ngày
        if (thanhToanDto.TrangThai == "DaThanhToan")
        {
            existingThanhToan.NgayThanhToan = DateTime.Now;
        }

        await dbContext.SaveChangesAsync();
        return existingThanhToan;
    }

    // DELETE
    public async Task<ThanhToan> DeleteAsync(int id)
    {
        var existing = await dbContext.ThanhToan.FirstOrDefaultAsync(x => x.MaThanhToan == id);
        if (existing == null) return null;

        dbContext.ThanhToan.Remove(existing);
        await dbContext.SaveChangesAsync();
        return existing;
    }
}