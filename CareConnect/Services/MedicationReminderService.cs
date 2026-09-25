using CareConnect.Data;
using CareConnect.Models;
using CareConnect.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Services
{
    public class MedicationReminderService
        : BackgroundService
    {
        private readonly IHubContext<NotificationHub>
    _hubContext;
        private readonly IServiceScopeFactory
            _scopeFactory;

        private readonly ILogger<
            MedicationReminderService>
            _logger;


        public MedicationReminderService(
      IServiceScopeFactory scopeFactory,
      ILogger<MedicationReminderService> logger,
      IHubContext<NotificationHub> hubContext)
        {
            _scopeFactory = scopeFactory;

            _logger = logger;

            _hubContext = hubContext;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await KiemTraLichNhac(
                        stoppingToken);
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
                        "Lỗi khi kiểm tra lịch nhắc thuốc.");
                }


                // Cứ 30 giây kiểm tra một lần
                await Task.Delay(
                    TimeSpan.FromSeconds(30),
                    stoppingToken);
            }
        }


        private async Task KiemTraLichNhac(
            CancellationToken cancellationToken)
        {
            using IServiceScope scope =
                _scopeFactory.CreateScope();


            var context =
                scope.ServiceProvider
                    .GetRequiredService<
                        CareConnectDbContext>();


            DateTime now =
                VietnamClock.Now;

            DateTime today =
                now.Date;

            TimeSpan currentTime =
                now.TimeOfDay;


            var danhSach =
                await context.LichNhacThuocs

                    .Include(x =>
                        x.BenhNhan)

                    .ThenInclude(x =>
                        x!.KhachHang)

                    .Where(x =>

                        // Lịch đang bật
                        x.TrangThai

                        &&

                        // Bệnh nhân còn hoạt động
                        x.BenhNhan != null

                        &&

                        x.BenhNhan
                            .TrangThaiHoatDong

                        &&

                        // Bản đầu tiên chỉ xử lý hằng ngày
                        x.TanSuat == "Hằng ngày"

                        &&

                        // Đã tới ngày bắt đầu
                        x.NgayBatDau <= today

                        &&

                        // Chưa hết hạn
                        (
                            x.NgayKetThuc == null
                            ||
                            x.NgayKetThuc >= today
                        )

                        &&

                        // Đã tới giờ uống
                        x.ThoiGianUong <= currentTime

                        &&

                        // Hôm nay chưa nhắc
                        (
                            x.LanNhacGanNhat == null
                            ||
                            x.LanNhacGanNhat.Value.Date
                                < today
                        )
                    )
                    .ToListAsync(
                        cancellationToken);

            var thongBaoRealtime =
                new List<(int MaTaiKhoan, ThongBao ThongBao)>();
            foreach (var lich in danhSach)
            {
                if (
                    lich.BenhNhan == null
                    ||
                    lich.BenhNhan.KhachHang == null)
                {
                    continue;
                }


                int maTaiKhoan =
                    lich.BenhNhan
                        .KhachHang
                        .MaTaiKhoan;


                string noiDung =
                    $"{lich.BenhNhan.HoTen}: " +
                    $"đến giờ uống {lich.TenThuoc}";


                if (!string.IsNullOrWhiteSpace(
                    lich.LieuDung))
                {
                    noiDung +=
                        $" ({lich.LieuDung})";
                }


                noiDung += ".";


                if (!string.IsNullOrWhiteSpace(
                    lich.CachDung))
                {
                    noiDung +=
                        $" {lich.CachDung}.";
                }


                var thongBao =
                    new ThongBao
                    {
                        MaTaiKhoan =
                            maTaiKhoan,

                        TieuDe =
                            "Nhắc uống thuốc",

                        NoiDung =
                            noiDung,

                        DaDoc =
                            false,

                        NgayGui =
                            now
                    };


                    context.ThongBaos.Add(
                        thongBao);


                    thongBaoRealtime.Add(
                        (
                            maTaiKhoan,
                            thongBao
                        ));


                    lich.LanNhacGanNhat =
                        now;
            }


            if (danhSach.Count > 0)
            {
                // Lưu DB trước
                await context.SaveChangesAsync(
                    cancellationToken);


                // Sau khi lưu thành công
                // mới phát realtime
                foreach (var item in thongBaoRealtime)
                {
                    await _hubContext
                        .Clients
                        .User(
                            item.MaTaiKhoan.ToString())
                        .SendAsync(
                            "ReceiveNotification",

                            new
                            {
                                maThongBao =
                                    item.ThongBao.MaThongBao,

                                tieuDe =
                                    item.ThongBao.TieuDe,

                                noiDung =
                                    item.ThongBao.NoiDung,

                                ngayGui =
                                    item.ThongBao.NgayGui
                            },

                            cancellationToken);
                }
            }
        }
    }
}