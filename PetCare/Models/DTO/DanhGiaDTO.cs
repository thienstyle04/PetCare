namespace PetCare.Models.DTO
{
    public class DanhGiaDTO
    {
        public int MaDanhGia { get; set; }
        public int MaLichHen { get; set; }
        public int MaKhachHang { get; set; }
        public int DiemSo { get; set; }
        public string? BinhLuan { get; set; }
        public DateTime NgayTao { get; set; }

        // Navigation properties for display
        public string? TenKhachHang { get; set; }
        public string? TenDichVu { get; set; }
    }
}
