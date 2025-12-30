using Microsoft.EntityFrameworkCore;
using PetCare.Data;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.Collections.Generic;
using System.Linq;

namespace PetCare.Repositories
{
    public class ThuCungRepositories : IThuCungRepositories
    {
        private readonly AppDbContext _dbContext;

        public ThuCungRepositories(AppDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public ThuCung AddPet(ThuCungDTO petDto)
        {
            // Kiểm tra xem khách hàng có tồn tại không
            var khachHangExists = _dbContext.KhachHang.Any(k => k.MaKhachHang == petDto.MaKhachHang);
            if (!khachHangExists)
            {
                throw new Exception($"Khách hàng với mã {petDto.MaKhachHang} không tồn tại.");
            }

            var petDomain = new ThuCung
            {
                MaKhachHang = petDto.MaKhachHang,
                TenThuCung = petDto.TenThuCung,
                Giong = petDto.Giong,
                ChuThich = petDto.ChuThich,
                Tuoi = petDto.Tuoi, // ✅ Lưu tuổi trực tiếp
                CanNang = petDto.CanNang,
                NgayTao = DateTime.Now
            };

            _dbContext.ThuCung.Add(petDomain);
            _dbContext.SaveChanges();
            return petDomain;
        }
        public IEnumerable<ThuCung> GetAllPets(
            string? filterOn = null, string? filterQuery = null,
            string? sortBy = null, bool isAscending = true,
            int pageNumber = 1, int pageSize = 10)
        {
            var pets = _dbContext.ThuCung.AsQueryable();

            // FILTERING
            if (!string.IsNullOrWhiteSpace(filterOn) && !string.IsNullOrWhiteSpace(filterQuery))
            {
                if (filterOn.Equals("TenThuCung", System.StringComparison.OrdinalIgnoreCase))
                {
                    pets = pets.Where(x => x.TenThuCung.Contains(filterQuery));
                }
                else if (filterOn.Equals("Giong", System.StringComparison.OrdinalIgnoreCase))
                {
                    pets = pets.Where(x => x.Giong != null && x.Giong.Contains(filterQuery));
                }
                else if (filterOn.Equals("MaKhachHang", System.StringComparison.OrdinalIgnoreCase))
                {
                    if (int.TryParse(filterQuery, out int maKhachHang))
                    {
                        pets = pets.Where(x => x.MaKhachHang == maKhachHang);
                    }
                }
            }

            // SORTING
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                if (sortBy.Equals("Ten", System.StringComparison.OrdinalIgnoreCase))
                {
                    pets = isAscending ? pets.OrderBy(x => x.TenThuCung) : pets.OrderByDescending(x => x.TenThuCung);
                }
                else if (sortBy.Equals("Tuoi", System.StringComparison.OrdinalIgnoreCase))
                {
                    pets = isAscending ? pets.OrderBy(x => x.Tuoi) : pets.OrderByDescending(x => x.Tuoi);
                }
            }

            // PAGINATION
            var skipResults = (pageNumber - 1) * pageSize;
            return pets.Skip(skipResults).Take(pageSize).ToList();
        }

        public ThuCung? GetPetById(int maThuCung)
        {
            return _dbContext.ThuCung.FirstOrDefault(x => x.MaThuCung == maThuCung);
        }

        public ThuCung? UpdatePet(int maThuCung, ThuCungDTO petDto)
        {
            var existingPet = _dbContext.ThuCung.FirstOrDefault(x => x.MaThuCung == maThuCung);

            if (existingPet == null) return null;

            existingPet.MaKhachHang = petDto.MaKhachHang;
            existingPet.TenThuCung = petDto.TenThuCung;
            existingPet.Giong = petDto.Giong;
            existingPet.ChuThich = petDto.ChuThich;
            existingPet.Tuoi = petDto.Tuoi; // ✅ Lưu tuổi trực tiếp
            existingPet.CanNang = petDto.CanNang;

            _dbContext.SaveChanges();
            return existingPet;
        }
        public ThuCung? DeletePet(int maThuCung)
        {
            var pet = _dbContext.ThuCung.FirstOrDefault(x => x.MaThuCung == maThuCung);
            if (pet == null) return null;

            // Kiểm tra xem có lịch hẹn nào đang liên quan đến thú cưng này không
            var hasLichHen = _dbContext.LichHen.Any(lh => lh.MaThuCung == maThuCung);
            if (hasLichHen)
            {
                throw new InvalidOperationException($"Không thể xóa thú cưng '{pet.TenThuCung}' vì đang có lịch hẹn liên quan. Vui lòng xóa các lịch hẹn trước.");
            }

            // Xóa các hình ảnh liên quan (nếu có)
            var relatedImages = _dbContext.Images.Where(i => i.MaThuCung == maThuCung).ToList();
            if (relatedImages.Any())
            {
                _dbContext.Images.RemoveRange(relatedImages);
            }

            _dbContext.ThuCung.Remove(pet);
            _dbContext.SaveChanges();
            return pet;
        }
    }
}