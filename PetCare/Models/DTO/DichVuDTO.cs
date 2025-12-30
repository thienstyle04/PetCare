namespace PetCare.Models.DTO
{
    public class DichVuDTO
    {
        public int MaDichVu { get; set; }
        public string TenDichVu { get; set; }
        public string? MoTa { get; set; }
        public decimal Gia { get; set; }
        public int? ThoiGian { get; set; }
    }
    public class DichVUPriceDTO
    {
        public string TenDichVu { get; set; }
        public decimal Gia { get; set; }
    }
    public class DichVuComBoDTO
    {
        public int MaDichVu { get; set; }
        public string TenDichVu { get; set; }
        public decimal Gia { get; set; }
        public int? ThoiGian { get; set; } // Thời gian tính bằng phút
    }
}
