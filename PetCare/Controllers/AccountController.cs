

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using PetCare.Repositories;

namespace PetCare.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ITokenRepositories _tokenRepositories;
        private readonly AppDbContext _dbcontext;
        private readonly ILogger<AccountController> _logger; // <-- THÊM Logger

        public AccountController(
            UserManager<IdentityUser> userManager,
            ITokenRepositories tokenRepositories,
            AppDbContext dbContext,
            ILogger<AccountController> logger) // <-- Inject Logger
        {
            _userManager = userManager;
            _tokenRepositories = tokenRepositories;
            _dbcontext = dbContext;
            _logger = logger; // Gán Logger
        }

        // POST: api/Account/Register
        [HttpPost]
        [Route("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterNguoiDungDTO registerNguoiDungDTO)
        {
            _logger.LogWarning("Attempting to register user: {Username}", registerNguoiDungDTO.TenDangNhap);

            if (!ModelState.IsValid) // <-- THÊM: Model Validation
            {
                _logger.LogError("Registration validation failed for user {Username}. Errors: {@Errors}", registerNguoiDungDTO.TenDangNhap, ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            var identityUser = new IdentityUser
            {
                UserName = registerNguoiDungDTO.TenDangNhap,
                Email = registerNguoiDungDTO.Email
            };

            var identityResult = await _userManager.CreateAsync(identityUser, registerNguoiDungDTO.MatKhau);

            if (identityResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(identityUser, "KHACHHANG");

                // Lưu vào bảng NguoiDung custom
                var nguoiDung = new NguoiDung
                {
                    IdentityUserId = identityUser.Id,
                    TenDangNhap = registerNguoiDungDTO.TenDangNhap,
                    Email = registerNguoiDungDTO.Email,
                    VaiTro = "KHACHHANG",
                    TrangThai = true,
                    NgayTao = DateTime.Now,
                    NgayCapNhat = DateTime.Now,
                    MatKhauHash = identityUser.PasswordHash
                };

                _dbcontext.NguoiDung.Add(nguoiDung);
                await _dbcontext.SaveChangesAsync();
                var khachHang = new KhachHang
                {
                    MaNguoiDung = nguoiDung.MaNguoiDung,
                    HoTen = registerNguoiDungDTO.HoTen,
                    NgayTao = DateTime.Now
                };

                _dbcontext.KhachHang.Add(khachHang);
                await _dbcontext.SaveChangesAsync();

                _logger.LogInformation("User {Username} registered successfully.", registerNguoiDungDTO.TenDangNhap);
                return Ok(new { Message = "Đăng ký thành công" });
            }
            else
            {
                _logger.LogError("Identity registration failed for user {Username}. Errors: {@IdentityErrors}", registerNguoiDungDTO.TenDangNhap, identityResult.Errors);
                return BadRequest(identityResult.Errors);
            }
        }

        // POST: api/Account/Login
        [HttpPost]
        [Route("Login")]
        public async Task<IActionResult> Login([FromBody] LoginNguoiDungDTO loginNguoiDungDTO)
        {
            _logger.LogInformation("Bắt đầu xử lý yêu cầu đăng nhập cho người dùng: {Username}", loginNguoiDungDTO.TenDangNhap);

            var identityUser = await _userManager.FindByNameAsync(loginNguoiDungDTO.TenDangNhap);

            if (identityUser != null)
            {
                var isPasswordValid = await _userManager.CheckPasswordAsync(identityUser, loginNguoiDungDTO.MatKhau);

                if (isPasswordValid)
                {
                    var roles = await _userManager.GetRolesAsync(identityUser);

                    // === SỬA ĐỔI BẮT ĐẦU TỪ ĐÂY ===

                    // 1. Tìm bản ghi NguoiDung tương ứng để lấy Id (chính là MaNguoiDung)
                    var nguoiDung = await _dbcontext.NguoiDung.FirstOrDefaultAsync(nd => nd.IdentityUserId == identityUser.Id);

                    if (nguoiDung == null)
                    {
                        _logger.LogWarning("Không tìm thấy thông tin người dùng trong bảng NguoiDung cho IdentityUserId: {IdentityUserId}", identityUser.Id);
                        return BadRequest(new { Message = "Lỗi dữ liệu người dùng." });
                    }

                    // 2. Tạo JWT Token
                    var token = _tokenRepositories.CreateJWTToken(identityUser, roles.ToList());

                    // 3. Tạo đối tượng Response để trả về
                    var response = new NguoiDungResponseDTO
                    {
                        JwtToken = token,
                        Roles = roles.ToList(),
                        MaNguoiDung = nguoiDung.MaNguoiDung // <-- Gán mã người dùng vào đây
                    };

                    // === KẾT THÚC SỬA ĐỔI ===

                    _logger.LogInformation("Người dùng {Username} đã đăng nhập thành công với vai trò: {Roles}", loginNguoiDungDTO.TenDangNhap, string.Join(", ", roles));
                    return Ok(response);
                }
            }

            _logger.LogWarning("Đăng nhập thất bại: Tên đăng nhập hoặc mật khẩu không chính xác cho {Username}.", loginNguoiDungDTO.TenDangNhap);
            return BadRequest(new { Message = "Tên đăng nhập hoặc mật khẩu không chính xác" });
        }

        // POST: api/Account/change-password
        [HttpPost]
        [Route("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDTO changePasswordDto)
        {
            _logger.LogInformation("Bắt đầu xử lý yêu cầu đổi mật khẩu cho MaNguoiDung: {MaNguoiDung}", changePasswordDto.MaNguoiDung);

            if (!ModelState.IsValid)
            {
                _logger.LogError("Change password validation failed for MaNguoiDung {MaNguoiDung}. Errors: {@Errors}", 
                    changePasswordDto.MaNguoiDung, ModelState.Values.SelectMany(v => v.Errors));
                return BadRequest(ModelState);
            }

            try
            {
                // Tìm NguoiDung để lấy IdentityUserId
                var nguoiDung = await _dbcontext.NguoiDung.FindAsync(changePasswordDto.MaNguoiDung);

                if (nguoiDung == null)
                {
                    _logger.LogWarning("Không tìm thấy người dùng với MaNguoiDung: {MaNguoiDung}", changePasswordDto.MaNguoiDung);
                    return NotFound(new { Message = "Không tìm thấy người dùng" });
                }

                // Tìm IdentityUser
                var identityUser = await _userManager.FindByIdAsync(nguoiDung.IdentityUserId);

                if (identityUser == null)
                {
                    _logger.LogWarning("Không tìm thấy IdentityUser với Id: {IdentityUserId}", nguoiDung.IdentityUserId);
                    return NotFound(new { Message = "Không tìm thấy tài khoản Identity" });
                }

                // Kiểm tra mật khẩu hiện tại
                var isOldPasswordValid = await _userManager.CheckPasswordAsync(identityUser, changePasswordDto.OldPassword);

                if (!isOldPasswordValid)
                {
                    _logger.LogWarning("Mật khẩu hiện tại không chính xác cho MaNguoiDung: {MaNguoiDung}", changePasswordDto.MaNguoiDung);
                    return BadRequest(new { Message = "Mật khẩu hiện tại không chính xác" });
                }

                // Đổi mật khẩu
                var changePasswordResult = await _userManager.ChangePasswordAsync(
                    identityUser, 
                    changePasswordDto.OldPassword, 
                    changePasswordDto.NewPassword
                );

                if (changePasswordResult.Succeeded)
                {
                    // Cập nhật MatKhauHash trong bảng NguoiDung
                    nguoiDung.MatKhauHash = identityUser.PasswordHash;
                    nguoiDung.NgayCapNhat = DateTime.Now;
                    await _dbcontext.SaveChangesAsync();

                    _logger.LogInformation("Đổi mật khẩu thành công cho MaNguoiDung: {MaNguoiDung}", changePasswordDto.MaNguoiDung);
                    return Ok(new { Message = "Đổi mật khẩu thành công" });
                }
                else
                {
                    _logger.LogError("Đổi mật khẩu thất bại cho MaNguoiDung: {MaNguoiDung}. Errors: {@IdentityErrors}", 
                        changePasswordDto.MaNguoiDung, changePasswordResult.Errors);
                    return BadRequest(new { Message = "Đổi mật khẩu thất bại", Errors = changePasswordResult.Errors });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi đổi mật khẩu cho MaNguoiDung: {MaNguoiDung}", changePasswordDto.MaNguoiDung);
                return StatusCode(500, new { Message = "Đã xảy ra lỗi khi đổi mật khẩu" });
            }
        }
    }
}