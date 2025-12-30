using System.ComponentModel.DataAnnotations;

namespace Petcare_web.Models.DTO
{
    public class DanhGiaRequestDTO
    {
        [Required]
        public int MaLichHen { get; set; }

        [Required]
        public int MaKhachHang { get; set; }

        [Required]
        [Range(1, 5)]
        public int DiemSo { get; set; }

        public string? BinhLuan { get; set; }
    }
}