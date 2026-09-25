using CareConnect.Data;
using CareConnect.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace CareConnect.Services
{
    public class PasswordSecurityService
        : IPasswordSecurityService
    {
        private readonly CareConnectDbContext _context;
        private readonly PasswordHasher<TaiKhoan> _passwordHasher;

        public PasswordSecurityService(
            CareConnectDbContext context)
        {
            _context = context;
            _passwordHasher =
                new PasswordHasher<TaiKhoan>();
        }

        public bool IsPasswordHash(
            string? storedPassword)
        {
            if (string.IsNullOrWhiteSpace(
                    storedPassword))
            {
                return false;
            }

            try
            {
                byte[] payload =
                    Convert.FromBase64String(
                        storedPassword);

                /*
                 * ASP.NET Core Identity:
                 * - 0x00: Identity V2
                 * - 0x01: Identity V3
                 */
                return payload.Length >= 49
                    &&
                    (
                        payload[0] == 0x00
                        ||
                        payload[0] == 0x01
                    );
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public string HashPassword(
            TaiKhoan taiKhoan,
            string plainPassword)
        {
            if (string.IsNullOrWhiteSpace(
                    plainPassword))
            {
                throw new ArgumentException(
                    "Mật khẩu không được để trống.",
                    nameof(plainPassword));
            }

            return _passwordHasher.HashPassword(
                taiKhoan,
                plainPassword);
        }

        public async Task<bool> VerifyPasswordAsync(
            TaiKhoan taiKhoan,
            string plainPassword)
        {
            if (taiKhoan == null
                ||
                string.IsNullOrWhiteSpace(
                    taiKhoan.MatKhau)
                ||
                string.IsNullOrEmpty(
                    plainPassword))
            {
                return false;
            }

            /*
             * Trường hợp tài khoản đã dùng hash
             * của ASP.NET Core Identity.
             */
            if (IsPasswordHash(
                    taiKhoan.MatKhau))
            {
                PasswordVerificationResult result =
                    _passwordHasher
                        .VerifyHashedPassword(
                            taiKhoan,
                            taiKhoan.MatKhau,
                            plainPassword);

                if (result
                    == PasswordVerificationResult.Failed)
                {
                    return false;
                }

                /*
                 * Nếu framework yêu cầu rehash,
                 * cập nhật hash mới ngay sau lần
                 * đăng nhập thành công.
                 */
                if (result
                    == PasswordVerificationResult
                        .SuccessRehashNeeded)
                {
                    string newHash =
                        HashPassword(
                            taiKhoan,
                            plainPassword);

                    await CapNhatHashAsync(
                        taiKhoan.MaTaiKhoan,
                        newHash);
                }

                return true;
            }

            /*
             * Tương thích tạm thời với dữ liệu cũ
             * đang lưu mật khẩu dạng thường.
             *
             * Nếu mật khẩu đúng, hệ thống nâng cấp
             * ngay sang hash và không lưu plaintext
             * trở lại database.
             */
            bool legacyPasswordMatches =
                FixedTimeLegacyCompare(
                    taiKhoan.MatKhau,
                    plainPassword);

            if (!legacyPasswordMatches)
            {
                return false;
            }

            string upgradedHash =
                HashPassword(
                    taiKhoan,
                    plainPassword);

            await CapNhatHashAsync(
                taiKhoan.MaTaiKhoan,
                upgradedHash);

            return true;
        }

        private async Task CapNhatHashAsync(
            int maTaiKhoan,
            string newHash)
        {
            await _context.TaiKhoans
                .Where(x =>
                    x.MaTaiKhoan
                        == maTaiKhoan)
                .ExecuteUpdateAsync(
                    setters =>
                        setters.SetProperty(
                            x => x.MatKhau,
                            newHash));
        }

        private static bool FixedTimeLegacyCompare(
            string storedPassword,
            string plainPassword)
        {
            byte[] storedDigest =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(
                        storedPassword));

            byte[] providedDigest =
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(
                        plainPassword));

            return CryptographicOperations
                .FixedTimeEquals(
                    storedDigest,
                    providedDigest);
        }
    }
}
