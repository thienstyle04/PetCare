using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.Domain
{
    
    public class NguoiDung
    {
        [Key]
        public int MaNguoiDung { get; set; }

        [Required]
        public string IdentityUserId { get; set; } // mapping với AspNetUsers table
        [Required]
        [MaxLength(50)]
        public string TenDangNhap { get; set; }

        [Required]
        [MaxLength(100)]
        public string Email { get; set; }

        [Required]
        [MaxLength(255)]
        public string MatKhauHash { get; set; }

        [Required]
        [MaxLength(20)]
        public string VaiTro { get; set; } // KhachHang, NhanVien, QuanTri

        public bool TrangThai { get; set; } = true;
        public DateTime NgayTao { get; set; } = DateTime.Now;
        public DateTime NgayCapNhat { get; set; } = DateTime.Now;

        // Navigation Properties
        public KhachHang? KhachHang { get; set; }
        public NhanVien? NhanVien { get; set; }

    }
}