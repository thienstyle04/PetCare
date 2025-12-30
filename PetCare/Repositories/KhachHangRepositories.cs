using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PetCare.Repositories
{
    public class KhachHangRepositories : IKhachHangRepositories
    {
        private readonly AppDbContext _dbContext;
        private readonly UserManager<IdentityUser> _userManager;
        

        public KhachHangRepositories(AppDbContext dbContext, UserManager<IdentityUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }
        public async Task<(NguoiDung user, KhachHang customer)> RegisterKhachHangAsync(RegisterKhachHangDTO dto)
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

            // 3️⃣ Tạo bản ghi NguoiDung
            var user = new NguoiDung
            {
                TenDangNhap = dto.TenDangNhap,
                Email = dto.Email,
                MatKhauHash = "", // Identity quản lý mật khẩu
                VaiTro = "KHACHHANG",
                TrangThai = true,
                NgayTao = DateTime.Now,
                NgayCapNhat = DateTime.Now,
                IdentityUserId = identityUser.Id
            };

            _dbContext.NguoiDung.Add(user);
            await _dbContext.SaveChangesAsync();

            // 4️⃣ Tạo KhachHang
            var khachHang = new KhachHang
            {
                MaNguoiDung = user.MaNguoiDung,
                HoTen = dto.HoTen,
                SoDienThoai = dto.SoDienThoai,
                DiaChi = dto.DiaChi,
                NgaySinh = dto.NgaySinh,
                NgayTao = DateTime.Now
            };

            _dbContext.KhachHang.Add(khachHang);
            await _dbContext.SaveChangesAsync();

            return (user, khachHang);
        }
    

        // Lấy danh sách khách hàng (lọc, sắp xếp, phân trang)
        public List<KhachHang> GetAllKhachHang(
            string? filterOn = null, string? filterQuery = null,
            string? sortBy = null, bool isAscending = true,
            int pageNumber = 1, int pageSize = 1000)
        {
            var allCustomers = _dbContext.KhachHang.AsQueryable();

            // Bộ lọc
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("HoTen", StringComparison.OrdinalIgnoreCase))
                {
                    allCustomers = allCustomers.Where(x => x.HoTen != null && x.HoTen.Contains(filterQuery));
                }
                else if (filterOn.Equals("SoDienThoai", StringComparison.OrdinalIgnoreCase))
                {
                    allCustomers = allCustomers.Where(x => x.SoDienThoai != null && x.SoDienThoai.Contains(filterQuery));
                }
            }

            // Sắp xếp
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("HoTen", StringComparison.OrdinalIgnoreCase))
                {
                    allCustomers = isAscending
                        ? allCustomers.OrderBy(x => x.HoTen)
                        : allCustomers.OrderByDescending(x => x.HoTen);
                }
                else if (sortBy.Equals("MaKhachHang", StringComparison.OrdinalIgnoreCase))
                {
                    allCustomers = isAscending
                        ? allCustomers.OrderBy(x => x.MaKhachHang)
                        : allCustomers.OrderByDescending(x => x.MaKhachHang);
                }
            }

            // Phân trang
            var skipResults = (pageNumber - 1) * pageSize;

            return allCustomers.Skip(skipResults).Take(pageSize).ToList();
        }

        // Lấy khách hàng theo ID
        public KhachHang? GetKhachHangById(int maKhachHang)
        {
            return _dbContext.KhachHang.FirstOrDefault(k => k.MaKhachHang == maKhachHang);
        }

        // Cập nhật thông tin khách hàng
        public KhachHang? UpdateKhachHang(int maKhachHang, KhachHangDTO updateDto)
        {
            var customer = _dbContext.KhachHang.FirstOrDefault(k => k.MaKhachHang == maKhachHang);
            if (customer == null) return null;

            // Cập nhật các trường
            customer.HoTen = updateDto.HoTen;
            customer.SoDienThoai = updateDto.SoDienThoai;
            customer.DiaChi = updateDto.DiaChi;
            customer.NgaySinh = updateDto.NgaySinh;

            _dbContext.SaveChanges();
            return customer;
        }

        // Xóa khách hàng
        public async Task<bool> DeleteKhachHangAsync(int maKhachHang)
        {
            var kh = _dbContext.KhachHang.FirstOrDefault(k => k.MaKhachHang == maKhachHang);
            if (kh == null) return false;

            // Lấy NguoiDung liên quan
            var user = _dbContext.NguoiDung.FirstOrDefault(n => n.MaNguoiDung == kh.MaNguoiDung);
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

            // EF sẽ tự cascade xóa KhachHang + NguoiDung nhờ cấu hình OnDelete.Cascade
            return true;
        }
    }
}
