using System.ComponentModel.DataAnnotations;

namespace Petcare_web.Models.DTO
{
    public class LichHenRequestDTO
    {
        [Required]
        public int MaKhachHang { get; set; }

        public int? MaNhanVien { get; set; }

        [Required]
        public int MaThuCung { get; set; }

        [Required]
        public int MaDichVu { get; set; }

        public string? NgayHen { get; set; }
        public string? GioHen { get; set; }

        [Required]
        public DateTime NgayGioHen { get; set; }

        public string? GhiChu { get; set; }
        public decimal? TongTien { get; set; }
        public string TrangThai { get; set; } = "Chờ xác nhận";
    }
}
