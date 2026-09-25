using CareConnect.Data;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Services
{
    public class LegacyPasswordMigrationService
        : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<
            LegacyPasswordMigrationService> _logger;

        public LegacyPasswordMigrationService(
            IServiceScopeFactory scopeFactory,
            ILogger<
                LegacyPasswordMigrationService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(
            CancellationToken cancellationToken)
        {
            using IServiceScope scope =
                _scopeFactory.CreateScope();

            var context =
                scope.ServiceProvider
                    .GetRequiredService<
                        CareConnectDbContext>();

            var passwordSecurity =
                scope.ServiceProvider
                    .GetRequiredService<
                        IPasswordSecurityService>();

            var taiKhoans =
                await context.TaiKhoans
                    .ToListAsync(
                        cancellationToken);

            int soTaiKhoanDaChuyenDoi = 0;

            foreach (var taiKhoan
                     in taiKhoans)
            {
                if (string.IsNullOrWhiteSpace(
                        taiKhoan.MatKhau))
                {
                    continue;
                }

                if (passwordSecurity
                    .IsPasswordHash(
                        taiKhoan.MatKhau))
                {
                    continue;
                }

                /*
                 * Database cũ của CareConnect đang
                 * lưu plaintext. Giữ nguyên mật khẩu
                 * người dùng nhưng thay giá trị lưu
                 * trong DB bằng hash an toàn.
                 */
                string plainPassword =
                    taiKhoan.MatKhau;

                taiKhoan.MatKhau =
                    passwordSecurity.HashPassword(
                        taiKhoan,
                        plainPassword);

                soTaiKhoanDaChuyenDoi++;
            }

            if (soTaiKhoanDaChuyenDoi > 0)
            {
                await context.SaveChangesAsync(
                    cancellationToken);

                _logger.LogInformation(
                    "Đã chuyển {Count} tài khoản "
                    + "từ mật khẩu dạng thường "
                    + "sang mật khẩu băm.",
                    soTaiKhoanDaChuyenDoi);
            }
        }

        public Task StopAsync(
            CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
