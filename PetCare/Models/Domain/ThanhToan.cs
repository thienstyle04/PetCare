using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCare.Models.Domain
{
    public class ThanhToan
    {
        [Key]
        public int MaThanhToan { get; set; }

        [Required]
        public int MaLichHen { get; set; }

        [Required]
        [Column(TypeName = "decimal(10,2)")]
        public decimal SoTien { get; set; }

        [Required]
        [MaxLength(20)]
        public string PhuongThuc { get; set; } // TienMat, TheNganHang, ChuyenKhoan, VNPay

        [Required]
        [MaxLength(20)]
        public string TrangThai { get; set; } = "ChoThanhToan"; // ChoThanhToan, DaThanhToan, ThatBai, HoanTien

        public DateTime? NgayThanhToan { get; set; }

        [MaxLength(255)]
        public string? GhiChu { get; set; }

        // Navigation Properties
        public LichHen LichHen { get; set; }
    }
}