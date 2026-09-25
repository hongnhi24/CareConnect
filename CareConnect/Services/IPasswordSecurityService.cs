using CareConnect.Models;

namespace CareConnect.Services
{
    public interface IPasswordSecurityService
    {
        bool IsPasswordHash(string? storedPassword);

        string HashPassword(
            TaiKhoan taiKhoan,
            string plainPassword);

        Task<bool> VerifyPasswordAsync(
            TaiKhoan taiKhoan,
            string plainPassword);
    }
}
