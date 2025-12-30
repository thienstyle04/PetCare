using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.Domain
{
    public class NhanVien
    {
        [Key]
        public int MaNhanVien { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        [Required]
        [MaxLength(100)]
        public string HoTen { get; set; }

        [MaxLength(15)]
        public string? SoDienThoai { get; set; }

        [MaxLength(50)]
        public string? ChucVu { get; set; } // TiepTan, NhanVienSpa, QuanLy

        public DateTime? NgayVaoLam { get; set; }

        public bool TrangThai { get; set; } = true;

        // Navigation Properties
        public NguoiDung NguoiDung { get; set; }

        // Reverse Navigation
        public ICollection<LichHen> LichHen { get; set; } = new List<LichHen>();
        public ICollection<HoSoDichVu> HoSoDichVu { get; set; } = new List<HoSoDichVu>();
    }
}