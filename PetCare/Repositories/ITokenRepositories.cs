using Microsoft.AspNetCore.Identity;
using PetCare.Models.Domain;
using PetCare.Models.DTO;
using System.Runtime.InteropServices;

namespace PetCare.Repositories
{
    public interface ITokenRepositories
    {
        string CreateJWTToken(IdentityUser NguoiDung, List<String> Role );
        List<NguoiDungDTO> Allnguoidung(string? filterOn = null,
            string? filterQuery = null,
            string? sortBy = null,
            bool isAscending = true,
            int pageNumber = 1,
            int pageSize = 100);
        NguoiDungDTO? GetNguoiDungById(int MaNguoiDung);
        NguoiDungDTO? updateNguoiDung(int MaNguoiDung, NguoiDungDTO nguoiDung);
        Task<NguoiDung?> DeleteNguoiDungAsync(int MaNguoiDung);
    }
}
