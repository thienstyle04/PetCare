using System.ComponentModel.DataAnnotations;

namespace Petcare_web.Models.DTO
{
    public class RegisterNguoiDungDTO
    {
        [Required(ErrorMessage = "Tên đăng nhập không được để trống")]
        [MaxLength(50)]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [MaxLength(100)]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string MatKhau { get; set; } // Nhận mật khẩu plain text
        public string HoTen { get; set; }

    }
}
