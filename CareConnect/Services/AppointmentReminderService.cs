using CareConnect.Data;
using CareConnect.Hubs;
using CareConnect.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Services
{
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly ILogger<AppointmentReminderService> _logger;

        public AppointmentReminderService(
            IServiceScopeFactory scopeFactory,
            IHubContext<NotificationHub> hubContext,
            ILogger<AppointmentReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await KiemTraLichNhac(stoppingToken);
                }
                catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex,
                        "Lỗi khi kiểm tra lịch nhắc tái khám.");
                }

                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken);
            }
        }

        private async Task KiemTraLichNhac(
            CancellationToken cancellationToken)
        {
            using IServiceScope scope = _scopeFactory.CreateScope();

            var context = scope.ServiceProvider
                .GetRequiredService<CareConnectDbContext>();

            DateTime now = VietnamClock.Now;
            DateTime today = now.Date;

            var danhSachUngVien = await context.LichNhacTaiKhams
                .Include(x => x.BenhNhan)
                .ThenInclude(x => x!.KhachHang)
                .Where(x =>
                    x.TrangThai
                    && x.LanNhacGanNhat == null
                    && x.BenhNhan != null
                    && x.BenhNhan.TrangThaiHoatDong
                    && x.NgayTaiKham >= today)
                .ToListAsync(cancellationToken);

            var canNhac = new List<LichNhacTaiKham>();

            foreach (var lich in danhSachUngVien)
            {
                DateTime thoiDiemTaiKham =
                    lich.NgayTaiKham.Date.Add(lich.GioTaiKham);

                DateTime thoiDiemNhac =
                    thoiDiemTaiKham.AddMinutes(-lich.SoPhutNhacTruoc);

                if (now >= thoiDiemNhac && now < thoiDiemTaiKham)
                {
                    canNhac.Add(lich);
                }
            }

            if (canNhac.Count == 0)
            {
                return;
            }

            var thongBaoRealtime =
                new List<(int MaTaiKhoan, ThongBao ThongBao)>();

            foreach (var lich in canNhac)
            {
                if (lich.BenhNhan?.KhachHang == null)
                {
                    continue;
                }

                int maTaiKhoan =
                    lich.BenhNhan.KhachHang.MaTaiKhoan;

                DateTime thoiDiemTaiKham =
                    lich.NgayTaiKham.Date.Add(lich.GioTaiKham);

                string noiDung =
                    $"{lich.BenhNhan.HoTen}: "
                    + $"có lịch tái khám \"{lich.NoiDung}\" "
                    + $"lúc {thoiDiemTaiKham:HH:mm dd/MM/yyyy}.";

                if (!string.IsNullOrWhiteSpace(lich.DiaDiem))
                {
                    noiDung += $" Địa điểm: {lich.DiaDiem}.";
                }

                if (!string.IsNullOrWhiteSpace(lich.GhiChu))
                {
                    noiDung += $" Ghi chú: {lich.GhiChu}.";
                }

                var thongBao = new ThongBao
                {
                    MaTaiKhoan = maTaiKhoan,
                    TieuDe = "Nhắc lịch tái khám",
                    NoiDung = noiDung,
                    DaDoc = false,
                    NgayGui = now
                };

                context.ThongBaos.Add(thongBao);
                lich.LanNhacGanNhat = now;

                thongBaoRealtime.Add((maTaiKhoan, thongBao));
            }

            await context.SaveChangesAsync(cancellationToken);

            foreach (var item in thongBaoRealtime)
            {
                try
                {
                    await _hubContext
                        .Clients
                        .User(item.MaTaiKhoan.ToString())
                        .SendAsync(
                            "ReceiveNotification",
                            new
                            {
                                maThongBao = item.ThongBao.MaThongBao,
                                tieuDe = item.ThongBao.TieuDe,
                                noiDung = item.ThongBao.NoiDung,
                                ngayGui = item.ThongBao.NgayGui
                            },
                            cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        ex,
                        "Không thể gửi thông báo tái khám realtime cho tài khoản {MaTaiKhoan}.",
                        item.MaTaiKhoan);
                }
            }
        }
    }
}
