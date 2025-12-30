using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.DTO
{
    // DTO cho đăng ký nhân viên mới
    public class RegisterNhanVienDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc")]
        [MaxLength(50)]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string MatKhau { get; set; }

        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [MaxLength(100)]
        public string HoTen { get; set; }

        [MaxLength(15)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [MaxLength(50)]
        public string? ChucVu { get; set; } // TiepTan, NhanVienSpa, QuanLy

        public DateTime? NgayVaoLam { get; set; }
    }

    // DTO cho cập nhật thông tin nhân viên
    public class NhanVienDTO
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [MaxLength(100)]
        public string HoTen { get; set; }

        [MaxLength(15)]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        public string? SoDienThoai { get; set; }

        [MaxLength(50)]
        public string? ChucVu { get; set; }

        public DateTime? NgayVaoLam { get; set; }

        public bool TrangThai { get; set; } = true;
    }
}