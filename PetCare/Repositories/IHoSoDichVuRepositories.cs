using PetCare.Models.Domain;
using PetCare.Models.DTO;

namespace PetCare.Repositories
{
    public interface IHoSoDichVuRepositories
    {
        List<HoSoDichVuDTO> AllHoSoDichVu(string? filterOn = null,
            string? filterQuery = null, string? sortBy = null, bool isAscending = true,
            int pageNumber = 1, int pageSize = 100);

        HoSoDichVuDTO AddHoSoDichVu(HoSoDichVuDTO dto);
        HoSoDichVuDTO? GetHoSoDichVuById(int MaHoSo);
        HoSoDichVuDTO? UpdateHoSoDichVu(int MaHoSo, HoSoDichVuDTO dto);
        HoSoDichVu? DeleteHoSoDichVu(int MaHoSo);
    }
}
