using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace PetCare.Models.Domain
{
    public class ThuCung
    {
        [Key]
        public int MaThuCung { get; set; }

        [Required]
        public int MaKhachHang { get; set; }

        [Required]
        [MaxLength(50)]
        public string TenThuCung { get; set; }

        [MaxLength(30)]
        public string? Loai { get; set; } // Chó, Mèo, Khác

        [MaxLength(30)]
        public string? Giong { get; set; }

        [MaxLength(10)]
        public string? GioiTinh { get; set; } // Đực, Cái

        public DateTime? NgaySinh { get; set; }
        
        public int? NamSinh { get; set; }

        [MaxLength(100)]
        public string? ChuThich { get; set; }

        public int? Tuoi { get; set; }

        [Column(TypeName = "decimal(7,2)")]
        public decimal? CanNang { get; set; } // Tính bằng kg

        [MaxLength(255)]
        public string? HinhAnh { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Properties
        [JsonIgnore]
        public KhachHang KhachHang { get; set; }

        // Reverse Navigation
        public ICollection<LichHen> LichHen { get; set; } = new List<LichHen>();
    }
}