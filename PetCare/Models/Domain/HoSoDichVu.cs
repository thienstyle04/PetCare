using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.Domain
{
    public class HoSoDichVu
    {
        [Key]
        public int MaHoSo { get; set; }

        [Required]
        public int MaLichHen { get; set; }

        [Required]
        public int MaNhanVien { get; set; }

        [MaxLength(255)]
        public string? AnhTruoc { get; set; } // URL ảnh trước khi làm

        [MaxLength(255)]
        public string? AnhSau { get; set; } // URL ảnh sau khi làm

        [MaxLength(1000)]
        public string? GhiChuDichVu { get; set; } // Ghi chép chi tiết quá trình

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Properties
        public LichHen LichHen { get; set; }
        public NhanVien NhanVien { get; set; }
    }
}