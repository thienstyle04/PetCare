using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.Domain
{
    public class KhachHang
    {
        [Key]
        public int MaKhachHang { get; set; }

        [Required]
        public int MaNguoiDung { get; set; }

        [Required]
        [MaxLength(100)]
        public string HoTen { get; set; }

        [MaxLength(15)]
        public string? SoDienThoai { get; set; }

        [MaxLength(200)]
        public string? DiaChi { get; set; }

        public DateTime? NgaySinh { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Properties
        public NguoiDung NguoiDung { get; set; }

        // Reverse Navigation
        public ICollection<ThuCung> ThuCung { get; set; } = new List<ThuCung>();
        public ICollection<LichHen> LichHen { get; set; } = new List<LichHen>();
        public ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();
    }
}