using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PetCare.Models.Domain
{
    public class DichVu
    {
        [Key]
        public int MaDichVu { get; set; }

        [Required]
        [MaxLength(100)]
        public string TenDichVu { get; set; }

        [MaxLength(500)]
        public string? MoTa { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal Gia { get; set; }

        public int? ThoiGian { get; set; } // Thời gian tính bằng phút

        public bool TrangThai { get; set; } = true;

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Reverse Navigation
        public ICollection<LichHen> LichHen { get; set; } = new List<LichHen>( );
    }
}