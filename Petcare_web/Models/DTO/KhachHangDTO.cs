using System;
using System.ComponentModel.DataAnnotations;

namespace Petcare_web.Models.DTO
{
    public class RegisterKhachHangDTO
    {
        // Tài khoản đăng nhập
        [Required(ErrorMessage = "Tên đăng nhập là bắt buộc.")]
        [StringLength(50)]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [MinLength(6, ErrorMessage = "Mật khẩu ít nhất 6 ký tự.")]
        public string MatKhau { get; set; }

        // Thông tin khách hàng
        [Required(ErrorMessage = "Họ tên là bắt buộc.")]
        [StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        public DateTime? NgaySinh { get; set; }
    }
    public class KhachHangDTO
    {
        public int MaKhachHang { get; set; }

        // Nếu API có trả về liên kết với người dùng
        public int? MaNguoiDung { get; set; }

        public string HoTen { get; set; } = string.Empty;

        public string? SoDienThoai { get; set; }

        public string? DiaChi { get; set; }

        public DateTime? NgaySinh { get; set; }

        public DateTime NgayTao { get; set; }

        // Dữ liệu hiển thị bổ sung (tùy bạn muốn hiển thị gì)
        public string? Email { get; set; } // nếu API trả về email người dùng
        public string? TenDangNhap { get; set; } // nếu bạn muốn show tài khoản
    }
}
