using System.ComponentModel.DataAnnotations;

namespace Petcare_web.Models.DTO
{
    public class ThanhToanDTO
    {
        public int MaThanhToan { get; set; }
        public int MaLichHen { get; set; }
        public decimal SoTien { get; set; }
        public string PhuongThuc { get; set; }
        public string TrangThai { get; set; }
        public DateTime? NgayThanhToan { get; set; }
        public string? GhiChu { get; set; }
    }
}
