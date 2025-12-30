namespace PetCare.Models.DTO
{
    public class ImageDTO
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public string? FileDescription { get; set; }
        public string FileExtension { get; set; }
        public long FileSizeInBytes { get; set; }
        public string FilePath { get; set; }
        public DateTime UploadedAt { get; set; }
        public int? MaThuCung { get; set; }
    }
}
