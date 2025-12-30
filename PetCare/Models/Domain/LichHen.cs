using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCare.Models.Domain
{
    public class LichHen
    {
        [Key]
        public int MaLichHen { get; set; }

        [Required]
        public int MaKhachHang { get; set; }

        [Required]
        public int MaThuCung { get; set; }

        [Required]
        public int MaDichVu { get; set; }

        public int? MaNhanVien { get; set; } // Có thể chưa phân công nhân viên

        [Required]
        public DateTime NgayGioHen { get; set; }

        [Required]
        [MaxLength(20)]
        public string TrangThai { get; set; } = "ChoXacNhan"; // ChoXacNhan, DaXacNhan, DangThucHien, HoanThanh, DaHuy

        [MaxLength(500)]
        public string? GhiChu { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? TongTien { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Properties
        public KhachHang KhachHang { get; set; }
        public ThuCung ThuCung { get; set; }
        public DichVu DichVu { get; set; }
        public NhanVien? NhanVien { get; set; }

        // Reverse Navigation
        public ICollection<HoSoDichVu> HoSoDichVu { get; set; } = new List<HoSoDichVu>();
        public ICollection<ThanhToan> ThanhToan { get; set; } = new List<ThanhToan>();
        public ICollection<DanhGia> DanhGia { get; set; } = new List<DanhGia>();
    }
}