using System.ComponentModel.DataAnnotations;

namespace PetCare.Models.DTO
{
    public class LichHenDTO
    {
        public int MaLichHen { get; set; }
        public int MaKhachHang { get; set; }
        public int MaThuCung { get; set; }
        public int MaDichVu { get; set; }
        public int? MaNhanVien { get; set; }
        public DateTime NgayGioHen { get; set; }
        public string TrangThai { get; set; }
        public string? GhiChu { get; set; }
        public decimal? TongTien { get; set; }
        public DateTime NgayTao { get; set; }

        // Navigation properties for display
        public string? TenKhachHang { get; set; }
        public string? TenThuCung { get; set; }
        public string? TenDichVu { get; set; }
        public string? TenNhanVien { get; set; }
    }
}
