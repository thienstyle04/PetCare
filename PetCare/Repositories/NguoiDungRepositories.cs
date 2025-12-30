using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Linq;
using System.Collections.Generic;

namespace PetCare.Repositories
{
    public class NguoiDungRepositories : ITokenRepositories
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbcontext;
        // Cần inject UserManager để thực hiện các thao tác liên quan đến Identity (như kiểm tra mật khẩu, xóa user)
        private readonly UserManager<IdentityUser> _userManager;

        public NguoiDungRepositories(IConfiguration configuration, AppDbContext dbcontext, UserManager<IdentityUser> userManager)
        {
            _configuration = configuration;
            _dbcontext = dbcontext;
            _userManager = userManager;
        }
        public string CreateJWTToken(IdentityUser user, List<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.UserName ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // Đọc Key từ appsettings.json
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:key"])
            );

            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(15),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        public List<NguoiDungDTO> Allnguoidung(
            string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 100)
        {
            var allNguoiDung = _dbcontext.NguoiDung.Select(n => new NguoiDungDTO
            {
                MaNguoiDung = n.MaNguoiDung,
                TenDangNhap = n.TenDangNhap,
                Email = n.Email,
                VaiTro = n.VaiTro,
                TrangThai = n.TrangThai,
                NgayTao = n.NgayTao,
                NgayCapNhat = n.NgayCapNhat
            }).AsQueryable();

            // 1. FILTERING
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("tendangnhap", StringComparison.OrdinalIgnoreCase))
                {
                    allNguoiDung = allNguoiDung.Where(x => x.TenDangNhap.Contains(filterQuery));
                }
                else if (filterOn.Equals("email", StringComparison.OrdinalIgnoreCase))
                {
                    allNguoiDung = allNguoiDung.Where(x => x.Email != null && x.Email.Contains(filterQuery));
                }
            }

            // 2. SORTING
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("tendangnhap", StringComparison.OrdinalIgnoreCase))
                {
                    allNguoiDung = isAscending ? allNguoiDung.OrderBy(x => x.TenDangNhap) : allNguoiDung.OrderByDescending(x => x.TenDangNhap);
                }
                else if (sortBy.Equals("ngaytao", StringComparison.OrdinalIgnoreCase))
                {
                    allNguoiDung = isAscending ? allNguoiDung.OrderBy(x => x.NgayTao) : allNguoiDung.OrderByDescending(x => x.NgayTao);
                }
                else if (sortBy.Equals("email", StringComparison.OrdinalIgnoreCase))
                {
                    allNguoiDung = isAscending ? allNguoiDung.OrderBy(x => x.Email) : allNguoiDung.OrderByDescending(x => x.Email);
                }
            }

            // 3. PAGINATION
            var skipResults = (pageNumber - 1) * pageSize;
            return allNguoiDung.Skip(skipResults).Take(pageSize).ToList();
        }
        public NguoiDungDTO? GetNguoiDungById(int MaNguoiDung)
        {
            var NDBID = _dbcontext.NguoiDung.Where(n => n.MaNguoiDung == MaNguoiDung)
                .Select(ND => new NguoiDungDTO()
                {
                    MaNguoiDung = ND.MaNguoiDung,
                    TenDangNhap = ND.TenDangNhap,
                    // Không trả về mật khẩu hash qua DTO nếu không cần thiết
                    Email = ND.Email,
                    VaiTro = ND.VaiTro,
                    TrangThai = ND.TrangThai,
                    NgayTao = ND.NgayTao,
                    NgayCapNhat = ND.NgayCapNhat,
                }).FirstOrDefault();

            return NDBID;
        }
        public NguoiDungDTO? updateNguoiDung(int MaNguoiDung, NguoiDungDTO nguoiDung)
        {
            var UpdateND = _dbcontext.NguoiDung.FirstOrDefault(n => n.MaNguoiDung == MaNguoiDung);
            if (UpdateND == null) return null;

            // Cập nhật các trường trong bảng NguoiDung custom
            UpdateND.TenDangNhap = nguoiDung.TenDangNhap;
            UpdateND.Email = nguoiDung.Email;
            // KHÔNG NÊN CẬP NHẬT MATKHAUHASH TRỰC TIẾP Ở ĐÂY. Cần dùng UserManager.ChangePasswordAsync
            UpdateND.VaiTro = nguoiDung.VaiTro;
            UpdateND.TrangThai = nguoiDung.TrangThai;
            UpdateND.NgayCapNhat = DateTime.Now;

            _dbcontext.SaveChanges();

            // Nếu có cập nhật email/username, cần cập nhật AspNetUsers
            var identityUser = _userManager.Users.FirstOrDefault(u => u.Id == UpdateND.IdentityUserId);
            if (identityUser != null)
            {
                identityUser.Email = nguoiDung.Email;
                identityUser.UserName = nguoiDung.TenDangNhap;
                // Nếu MatKhau được gửi trong DTO, cần xử lý thay đổi mật khẩu qua UserManager
                // var result = await _userManager.UpdateAsync(identityUser); 
            }

            return new NguoiDungDTO
            {
                MaNguoiDung = UpdateND.MaNguoiDung,
                TenDangNhap = UpdateND.TenDangNhap,
                Email = UpdateND.Email,
                VaiTro = UpdateND.VaiTro,
                TrangThai = UpdateND.TrangThai,
                NgayTao = UpdateND.NgayTao,
                NgayCapNhat = UpdateND.NgayCapNhat
            };
        }
        public async Task<NguoiDung?> DeleteNguoiDungAsync(int maNguoiDung)
        {
            var userRecord = _dbcontext.NguoiDung.FirstOrDefault(n => n.MaNguoiDung == maNguoiDung);
            if (userRecord == null)
                return null;

            var identityUser = await _userManager.FindByIdAsync(userRecord.IdentityUserId);
            if (identityUser == null)
                return null;

            // 🔥 Xóa user trong Identity (EF tự cascade xóa NguoiDung + KhachHang)
            var result = await _userManager.DeleteAsync(identityUser);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Không thể xóa tài khoản Identity: {errors}");
            }

            return userRecord;
        }
    }
}