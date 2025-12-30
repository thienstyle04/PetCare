using System.ComponentModel.DataAnnotations;

namespace Petcare_web.Models.DTO
{
    public class LoginNguoiDungDTO
    {
        [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
        [Display(Name = "Tên đăng nhập")]
        public string TenDangNhap { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [Display(Name = "Mật khẩu")]
        public string MatKhau { get; set; }
    }
}
