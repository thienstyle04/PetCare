using System;
using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.DTO
{
    public class KhachHangDTO
    {

        public int? MaKhachHang { get; set; }

        [Required]
        public string HoTen { get; set; } = string.Empty;

        public string? SoDienThoai { get; set; }
        public string? DiaChi { get; set; }
        public DateTime? NgaySinh { get; set; }
    }
    public class RegisterKhachHangDTO
    {
        // Dữ liệu tài khoản người dùng
        [Required]
        public string TenDangNhap { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [MinLength(6)]
        public string MatKhau { get; set; }

        // Dữ liệu khách hàng
        [Required]
        [StringLength(100)]
        public string HoTen { get; set; }

        [StringLength(15)]
        public string? SoDienThoai { get; set; }

        [StringLength(255)]
        public string? DiaChi { get; set; }

        public DateTime? NgaySinh { get; set; }
    }
}
