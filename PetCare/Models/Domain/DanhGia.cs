using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.Domain
{
    public class DanhGia
    {
        [Key]
        public int MaDanhGia { get; set; }

        [Required]
        public int MaLichHen { get; set; }

        [Required]
        public int MaKhachHang { get; set; }

        [Required]
        [Range(1, 5)]
        public int DiemSo { get; set; } // 1-5 stars

        [MaxLength(1000)]
        public string? BinhLuan { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Navigation Properties - không required vì sẽ được load từ DB
        public LichHen? LichHen { get; set; }
        public KhachHang? KhachHang { get; set; }
    }
}