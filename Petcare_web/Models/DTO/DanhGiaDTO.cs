using System.ComponentModel.DataAnnotations;

namespace Petcare_web.Models.DTO
{
    public class DanhGiaDTO
    {
        public int MaDanhGia { get; set; }

        [Required(ErrorMessage = "Mã lịch hẹn là bắt buộc")]
        public int MaLichHen { get; set; }

        [Required(ErrorMessage = "Mã khách hàng là bắt buộc")]
        public int MaKhachHang { get; set; }

        [Required(ErrorMessage = "Điểm số là bắt buộc")]
        [Range(1, 5, ErrorMessage = "Điểm số phải từ 1 đến 5 sao")]
        public int DiemSo { get; set; }

        [MaxLength(1000, ErrorMessage = "Bình luận không được vượt quá 1000 ký tự")]
        public string? BinhLuan { get; set; }

        public DateTime NgayTao { get; set; }

        // Navigation properties for display
        public string? TenKhachHang { get; set; }
        public string? TenDichVu { get; set; }
    }

    public class LichHenReviewDTO
    {
        public int MaLichHen { get; set; }
        public DateTime NgayHen { get; set; }
        public TimeSpan GioHen { get; set; }
        public string? TenDichVu { get; set; }
        public string? TenThuCung { get; set; }
        public decimal GiaDichVu { get; set; }
        public bool DaDanhGia { get; set; }
        public int? DiemSo { get; set; }
        public string? BinhLuan { get; set; }
    }
}
