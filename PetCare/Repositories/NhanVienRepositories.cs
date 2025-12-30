using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace PetCare.Repositories
{
    public class NhanVienRepositories : INhanVienRepositories
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;

        public NhanVienRepositories(AppDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<(NguoiDung user, NhanVien employee)> RegisterNhanVienAsync(RegisterNhanVienDTO dto)
        {
            // 1️⃣ Kiểm tra trùng tên đăng nhập hoặc email
            if (await _userManager.FindByNameAsync(dto.TenDangNhap) != null)
                throw new Exception("Tên đăng nhập đã tồn tại.");
            if (await _userManager.FindByEmailAsync(dto.Email) != null)
                throw new Exception("Email đã được sử dụng.");

            // 2️⃣ Tạo tài khoản Identity
            var identityUser = new IdentityUser
            {
                UserName = dto.TenDangNhap,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(identityUser, dto.MatKhau);
            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                throw new Exception($"Không thể tạo tài khoản: {errors}");
            }

            // Thêm role NhanVien
            await _userManager.AddToRoleAsync(identityUser, "NhanVien");

            // 3️⃣ Tạo bản ghi NguoiDung
            var user = new NguoiDung
            {
                TenDangNhap = dto.TenDangNhap,
                Email = dto.Email,
                MatKhauHash = "", // Identity quản lý mật khẩu
                VaiTro = "NHANVIEN",
                TrangThai = true,
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now,
                IdentityUserId = identityUser.Id
            };

            _dbContext.NguoiDung.Add(user);
            await _dbContext.SaveChangesAsync();

            // 4️⃣ Tạo NhanVien
            var nhanVien = new NhanVien
            {
                MaNguoiDung = user.MaNguoiDung,
                HoTen = dto.HoTen,
                SoDienThoai = dto.SoDienThoai,
                ChucVu = dto.ChucVu,
                NgayVaoLam = dto.NgayVaoLam ?? DateTime.Now,
                TrangThai = true
            };

            _dbContext.NhanVien.Add(nhanVien);
            await _dbContext.SaveChangesAsync();

            return (user, nhanVien);
        }

        // Lấy danh sách nhân viên (lọc, sắp xếp, phân trang)
        public List<NhanVien> GetAllNhanVien(
            string? filterOn = null, string? filterQuery = null,
            string? sortBy = null, bool isAscending = true,
            int pageNumber = 1, int pageSize = 1000)
        {
            var allEmployees = _dbContext.NhanVien
                .Include(nv => nv.NguoiDung)
                .AsQueryable();

            // Bộ lọc
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("HoTen", StringComparison.OrdinalIgnoreCase))
                {
                    allEmployees = allEmployees.Where(x => x.HoTen != null && x.HoTen.Contains(filterQuery));
                }
                else if (filterOn.Equals("SoDienThoai", StringComparison.OrdinalIgnoreCase))
                {
                    allEmployees = allEmployees.Where(x => x.SoDienThoai != null && x.SoDienThoai.Contains(filterQuery));
                }
                else if (filterOn.Equals("ChucVu", StringComparison.OrdinalIgnoreCase))
                {
                    allEmployees = allEmployees.Where(x => x.ChucVu != null && x.ChucVu.Contains(filterQuery));
                }
            }

            // Sắp xếp
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("HoTen", StringComparison.OrdinalIgnoreCase))
                {
                    allEmployees = isAscending
                        ? allEmployees.OrderBy(x => x.HoTen)
                        : allEmployees.OrderByDescending(x => x.HoTen);
                }
                else if (sortBy.Equals("MaNhanVien", StringComparison.OrdinalIgnoreCase))
                {
                    allEmployees = isAscending
                        ? allEmployees.OrderBy(x => x.MaNhanVien)
                        : allEmployees.OrderByDescending(x => x.MaNhanVien);
                }
                else if (sortBy.Equals("NgayVaoLam", StringComparison.OrdinalIgnoreCase))
                {
                    allEmployees = isAscending
                        ? allEmployees.OrderBy(x => x.NgayVaoLam)
                        : allEmployees.OrderByDescending(x => x.NgayVaoLam);
                }
            }

            // Phân trang
            var skipResults = (pageNumber - 1) * pageSize;

            return allEmployees.Skip(skipResults).Take(pageSize).ToList();
        }

        // Lấy nhân viên theo ID
        public NhanVien? GetNhanVienById(int maNhanVien)
        {
            return _dbContext.NhanVien
                .Include(nv => nv.NguoiDung)
                .FirstOrDefault(nv => nv.MaNhanVien == maNhanVien);
        }
        public NhanVien? GetNhanVienByUserId(int maNguoiDung)
        {
            return _dbContext.NhanVien
                .Include(nv => nv.NguoiDung)
                .FirstOrDefault(nv => nv.MaNguoiDung == maNguoiDung);
        }

        public NhanVien? GetNhanVienByIdentityUserId(string identityUserId)
        {
            return _dbContext.NhanVien
                .Include(nv => nv.NguoiDung)
                .FirstOrDefault(nv => nv.NguoiDung != null && nv.NguoiDung.IdentityUserId == identityUserId);
        }

        // Cập nhật thông tin nhân viên
        public NhanVien? UpdateNhanVien(int maNguoiDung, NhanVienDTO updateDto)
        {
            var employee = _dbContext.NhanVien.FirstOrDefault(nv => nv.MaNhanVien == maNguoiDung);
            if (employee == null) return null;

            // Cập nhật các trường
            employee.HoTen = updateDto.HoTen;
            employee.SoDienThoai = updateDto.SoDienThoai;
            employee.ChucVu = updateDto.ChucVu;
            employee.NgayVaoLam = updateDto.NgayVaoLam;
            employee.TrangThai = updateDto.TrangThai;

            _dbContext.SaveChanges();
            return employee;
        }

        // Xóa nhân viên
        public async Task<bool> DeleteNhanVienAsync(int maNhanVien)
        {
            var nv = _dbContext.NhanVien.FirstOrDefault(n => n.MaNhanVien == maNhanVien);
            if (nv == null) return false;

            // Lấy NguoiDung liên quan
            var user = _dbContext.NguoiDung.FirstOrDefault(n => n.MaNguoiDung == nv.MaNguoiDung);
            if (user == null) return false;

            // Tìm IdentityUser tương ứng
            var identityUser = await _userManager.FindByIdAsync(user.IdentityUserId);
            if (identityUser == null) return false;

            // Xóa user trong Identity
            var result = await _userManager.DeleteAsync(identityUser);
            if (!result.Succeeded)
            {
                throw new Exception(string.Join(", ", result.Errors.Select(e => e.Description)));
            }

            // EF sẽ tự cascade xóa NhanVien + NguoiDung nhờ cấu hình OnDelete.Cascade
            return true;
        }
    }
}