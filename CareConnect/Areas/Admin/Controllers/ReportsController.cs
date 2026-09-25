using CareConnect.Data;
using CareConnect.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Quản trị viên")]
    public class ReportsController : Controller
    {
        private readonly CareConnectDbContext _context;

        public ReportsController(
            CareConnectDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> Index(
            int? nam,
            string? phamVi)
        {
            /*
             * 1. Xử lý năm và phạm vi báo cáo.
             */

            int namDangChon =
                nam ?? DateTime.Today.Year;

            phamVi =
                string.IsNullOrWhiteSpace(phamVi)
                    ? "all"
                    : phamVi.Trim().ToLower();

            if (phamVi != "all" &&
                phamVi != "6" &&
                phamVi != "3")
            {
                phamVi = "all";
            }

            DateTime dauNam =
                new DateTime(
                    namDangChon,
                    1,
                    1);

            DateTime dauNamSau =
                dauNam.AddYears(1);

            DateTime ngayHienTai =
    DateTime.Today;

            DateTime dauThangHienTai =
                new DateTime(
                    ngayHienTai.Year,
                    ngayHienTai.Month,
                    1);

            DateTime ngayBatDauPhamVi;

            DateTime ngayKetThucPhamVi;

            if (phamVi == "3")
            {
                // Tháng hiện tại + 2 tháng trước
                ngayBatDauPhamVi =
                    dauThangHienTai.AddMonths(-2);

                ngayKetThucPhamVi =
                    dauThangHienTai.AddMonths(1);
            }
            else if (phamVi == "6")
            {
                // Tháng hiện tại + 5 tháng trước
                ngayBatDauPhamVi =
                    dauThangHienTai.AddMonths(-5);

                ngayKetThucPhamVi =
                    dauThangHienTai.AddMonths(1);
            }
            else
            {
                // Toàn bộ năm đang chọn
                ngayBatDauPhamVi =
                    dauNam;

                ngayKetThucPhamVi =
                    dauNamSau;
            }            

            DateTime dauThangHienTaiSau =
                dauThangHienTai.AddMonths(1);

            /*
             * 2. Lấy toàn bộ lịch trong năm được chọn.
             */

            var lichTrongNam =
                await (
                    from lich in _context.DatLichs
                        .AsNoTracking()

                    join khachHang in _context.KhachHangs
                        .AsNoTracking()
                        on lich.MaKhachHang
                        equals khachHang.MaKhachHang

                    join benhNhan in _context.BenhNhans
                        .AsNoTracking()
                        on lich.MaBenhNhan
                        equals benhNhan.MaBenhNhan

                    join nguoiChamSocTam
                        in _context.NguoiChamSocs
                            .AsNoTracking()
                        on lich.MaNguoiChamSoc
                        equals nguoiChamSocTam.MaNguoiChamSoc
                        into nhomNguoiChamSoc

                    from nguoiChamSoc
                        in nhomNguoiChamSoc.DefaultIfEmpty()

                    where lich.NgayChamSoc.HasValue
      &&
      lich.NgayChamSoc.Value >= ngayBatDauPhamVi
      &&
      lich.NgayChamSoc.Value < ngayKetThucPhamVi

                    select new AdminRecentBookingReportItemViewModel
                    {
                        MaDatLich =
                            lich.MaDatLich,

                        TenKhachHang =
                            string.IsNullOrWhiteSpace(
                                khachHang.HoTen)
                                ? "Khách hàng"
                                : khachHang.HoTen,

                        TenBenhNhan =
                            string.IsNullOrWhiteSpace(
                                benhNhan.HoTen)
                                ? "Người được chăm sóc"
                                : benhNhan.HoTen,

                        TenNguoiChamSoc =
                            nguoiChamSoc == null ||
                            string.IsNullOrWhiteSpace(
                                nguoiChamSoc.HoTen)
                                ? "Chưa phân công"
                                : nguoiChamSoc.HoTen,

                        KhuVuc =
                            string.IsNullOrWhiteSpace(
                                benhNhan.DiaChi)
                                ? "Chưa cập nhật"
                                : benhNhan.DiaChi,

                        NgayChamSoc =
                            lich.NgayChamSoc
                            ?? DateTime.Today,

                        GioBatDau =
                            lich.GioBatDau
                            ?? TimeSpan.Zero,

                        GioKetThuc =
                            lich.GioKetThuc
                            ?? TimeSpan.Zero,

                        TongTien =
                            lich.TongTien
                            ?? 0m,

                        TrangThai =
                            lich.TrangThai
                            ?? "Chưa xác định"
                    })
                    .ToListAsync();

            /*
             * Chuyển địa chỉ đầy đủ thành Quận/Huyện.
             */

            foreach (var item in lichTrongNam)
            {
                item.KhuVuc =
                    LayKhuVucTuDiaChi(
                        item.KhuVuc);
            }

            /*
             * 3. Lấy danh sách lịch theo phạm vi:
             * Tất cả, 6 tháng hoặc 3 tháng.
             */

            var lichTrongPhamVi =
                lichTrongNam
                    .Where(x =>
                        x.NgayChamSoc >= ngayBatDauPhamVi
                        &&
                        x.NgayChamSoc < ngayKetThucPhamVi)
                    .ToList();

            int tongLuotDatTrongKy =
                lichTrongPhamVi.Count;

            int soLichHoanThanh =
                lichTrongPhamVi.Count(x =>
                    x.TrangThai == "Đã hoàn thành");

            int soLichDaHuy =
                lichTrongPhamVi.Count(x =>
                    x.TrangThai == "Đã hủy");

            decimal tongDoanhThuTrongKy =
                lichTrongPhamVi
                    .Where(x =>
                        x.TrangThai == "Đã hoàn thành")
                    .Sum(x =>
                        x.TongTien);

            double tyLeHoanThanh =
                tongLuotDatTrongKy == 0
                    ? 0
                    : Math.Round(
                        soLichHoanThanh
                        * 100.0
                        / tongLuotDatTrongKy,
                        1);

            double tyLeHuy =
                tongLuotDatTrongKy == 0
                    ? 0
                    : Math.Round(
                        soLichDaHuy
                        * 100.0
                        / tongLuotDatTrongKy,
                        1);

            /*
             * 4. Lấy tài khoản mới trong năm.
             */

            var ngayTaoTaiKhoanTrongPhamVi =
     await _context.TaiKhoans
         .AsNoTracking()
         .Where(x =>
             x.NgayTao >= ngayBatDauPhamVi
             &&
             x.NgayTao < ngayKetThucPhamVi)
         .Select(x => x.NgayTao)
         .ToListAsync();

            int nguoiDungMoiTrongKy =
                ngayTaoTaiKhoanTrongPhamVi.Count(x =>
                    x >= ngayBatDauPhamVi
                    &&
                    x < ngayKetThucPhamVi);

            /*
             * 5. Báo cáo chi tiết theo tháng.
             */

            int soThang =
    phamVi switch
    {
        "3" => 3,
        "6" => 6,
        _ => 12
    };

            DateTime thangBatDau =
                phamVi switch
                {
                    "3" => dauThangHienTai.AddMonths(-2),
                    "6" => dauThangHienTai.AddMonths(-5),
                    _ => dauNam
                };

            var baoCaoTheoThang =
                new List<AdminMonthlyReportItemViewModel>();

            decimal doanhThuThangTruoc = 0m;

            for (int index = 0;
                 index < soThang;
                 index++)
            {
                DateTime dauThang =
                    thangBatDau.AddMonths(index);

                DateTime dauThangSau =
                    dauThang.AddMonths(1);

                var danhSachThang =
                    lichTrongNam
                        .Where(x =>
                            x.NgayChamSoc >= dauThang
                            &&
                            x.NgayChamSoc < dauThangSau)
                        .ToList();

                int soLich =
                    danhSachThang.Count;

                int soLichHoanThanhThang =
                    danhSachThang.Count(x =>
                        x.TrangThai == "Đã hoàn thành");

                decimal doanhThu =
                    danhSachThang
                        .Where(x =>
                            x.TrangThai == "Đã hoàn thành")
                        .Sum(x =>
                            x.TongTien);

                int nguoiDungMoi =
                    ngayTaoTaiKhoanTrongPhamVi.Count(x =>
                        x >= dauThang
                        &&
                        x < dauThangSau);

                decimal trungBinhMoiLuot =
                    soLich == 0
                        ? 0m
                        : doanhThu / soLich;

                double tangTruong =
                    doanhThuThangTruoc <= 0m
                        ? 0
                        : Math.Round(
                            (double)(
                                (doanhThu - doanhThuThangTruoc)
                                / doanhThuThangTruoc
                                * 100m),
                            1);

                baoCaoTheoThang.Add(
                    new AdminMonthlyReportItemViewModel
                    {
                        Thang =
                            dauThang.Month,

                        Nam =
                            dauThang.Year,

                        NhanThang =
                            dauThang.ToString("MM/yyyy"),

                        SoLich =
                            soLich,

                        SoLichHoanThanh =
                            soLichHoanThanhThang,

                        GiaTriHoanThanh =
                            doanhThu,

                        NguoiDungMoi =
                            nguoiDungMoi,

                        TrungBinhMoiLuot =
                            trungBinhMoiLuot,

                        TangTruong =
                            tangTruong
                    });

                doanhThuThangTruoc =
                    doanhThu;
            }

            /*
             * Bỏ tháng đầu tiên vì tháng đó chưa có
             * tháng trước để so sánh tăng trưởng.
             */

            var cacThangCoTangTruong =
                baoCaoTheoThang
                    .Skip(1)
                    .ToList();

            double tangTruongTrungBinh =
                cacThangCoTangTruong.Count == 0
                    ? 0
                    : Math.Round(
                        cacThangCoTangTruong
                            .Average(x =>
                                x.TangTruong),
                        1);

            /*
             * 6. Phân bố trạng thái lịch.
             */

            string[] cacTrangThai =
            {
                "Chờ xác nhận",
                "Đã xác nhận",
                "Đang thực hiện",
                "Đã hoàn thành",
                "Đã hủy"
            };

            var phanBoTrangThai =
                cacTrangThai
                    .Select(trangThai =>
                    {
                        int soLuong =
                            lichTrongPhamVi.Count(x =>
                                x.TrangThai == trangThai);

                        return new
                            AdminBookingStatusReportItemViewModel
                        {
                            TrangThai =
                                trangThai,

                            SoLuong =
                                soLuong,

                            TyLe =
                                tongLuotDatTrongKy == 0
                                    ? 0
                                    : Math.Round(
                                        soLuong
                                        * 100.0
                                        / tongLuotDatTrongKy,
                                        1)
                        };
                    })
                    .ToList();

            /*
             * 7. Báo cáo theo khu vực.
             */

            var baoCaoKhuVuc =
                lichTrongPhamVi
                    .Where(x =>
                        !string.IsNullOrWhiteSpace(
                            x.KhuVuc)
                        &&
                        x.KhuVuc != "Chưa cập nhật"
                        &&
                        x.KhuVuc != "Khu vực khác")
                    .GroupBy(x =>
                        x.KhuVuc)
                    .Select(group =>
                        new AdminAreaReportItemViewModel
                        {
                            KhuVuc =
                                group.Key,

                            SoLich =
                                group.Count(),

                            SoLichHoanThanh =
                                group.Count(x =>
                                    x.TrangThai
                                    == "Đã hoàn thành"),

                            TongGiaTri =
                                group
                                    .Where(x =>
                                        x.TrangThai
                                        == "Đã hoàn thành")
                                    .Sum(x =>
                                        x.TongTien)
                        })
                    .OrderByDescending(x =>
                        x.TongGiaTri)
                    .ThenByDescending(x =>
                        x.SoLich)
                    .Take(10)
                    .ToList();

            /*
  * 8. Báo cáo doanh thu theo dịch vụ.
  *
  * Hiển thị TOÀN BỘ dịch vụ trong hệ thống,
  * kể cả dịch vụ chưa có lượt đặt trong phạm vi đang chọn.
  */

            var danhSachDichVu =
                await _context.DichVus
                    .AsNoTracking()
                    .Select(x => new
                    {
                        x.MaDichVu,
                        x.TenDichVu
                    })
                    .ToListAsync();


            var duLieuDatLichTheoDichVu =
                await _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.NgayChamSoc.HasValue
                        &&
                        x.NgayChamSoc.Value
                            >= ngayBatDauPhamVi
                        &&
                        x.NgayChamSoc.Value
                            < ngayKetThucPhamVi)
                    .Select(x => new
                    {
                        x.MaDichVu,

                        TrangThai =
                            x.TrangThai
                            ?? string.Empty,

                        TongTien =
                            x.TongTien
                            ?? 0m
                    })
                    .ToListAsync();


            var thongKeDichVu =
                duLieuDatLichTheoDichVu
                    .GroupBy(x =>
                        x.MaDichVu)
                    .Select(group => new
                    {
                        MaDichVu =
                            group.Key,

                        SoLuotDat =
                            group.Count(),

                        TongDoanhThu =
                            group
                                .Where(x =>
                                    x.TrangThai
                                    == "Đã hoàn thành")
                                .Sum(x =>
                                    x.TongTien)
                    })
                    .ToList();


            var baoCaoDichVu =
                (
                    from dichVu in danhSachDichVu

                    join thongKe in thongKeDichVu
                        on dichVu.MaDichVu
                        equals thongKe.MaDichVu
                        into nhomThongKe

                    from thongKe
                        in nhomThongKe.DefaultIfEmpty()

                    select new AdminServiceReportItemViewModel
                    {
                        TenDichVu =
                            string.IsNullOrWhiteSpace(
                                dichVu.TenDichVu)
                                ? "Dịch vụ chưa đặt tên"
                                : dichVu.TenDichVu,

                        SoLuotDat =
                            thongKe == null
                                ? 0
                                : thongKe.SoLuotDat,

                        TongDoanhThu =
                            thongKe == null
                                ? 0m
                                : thongKe.TongDoanhThu
                    }
                )
                .OrderByDescending(x =>
                    x.TongDoanhThu)
                .ThenByDescending(x =>
                    x.SoLuotDat)
                .ThenBy(x =>
                    x.TenDichVu)
                .ToList();


            decimal doanhThuDichVuCaoNhat =
                baoCaoDichVu.Count == 0
                    ? 1m
                    : Math.Max(
                        1m,
                        baoCaoDichVu.Max(x =>
                            x.TongDoanhThu));


            int soLuotDatDichVuCaoNhat =
                baoCaoDichVu.Count == 0
                    ? 1
                    : Math.Max(
                        1,
                        baoCaoDichVu.Max(x =>
                            x.SoLuotDat));


            foreach (var item in baoCaoDichVu)
            {
                /*
                 * Nếu có doanh thu:
                 * thanh tiến độ dựa theo doanh thu.
                 *
                 * Nếu chưa có doanh thu nhưng có lượt đặt:
                 * dùng số lượt đặt.
                 *
                 * Nếu chưa có cả lượt đặt:
                 * thanh = 0%.
                 */

                if (item.TongDoanhThu > 0)
                {
                    item.TyLe =
                        Math.Round(
                            (double)(
                                item.TongDoanhThu
                                / doanhThuDichVuCaoNhat
                                * 100m),
                            1);
                }
                else if (item.SoLuotDat > 0)
                {
                    item.TyLe =
                        Math.Round(
                            item.SoLuotDat
                            * 100.0
                            / soLuotDatDichVuCaoNhat,
                            1);
                }
                else
                {
                    item.TyLe = 0;
                }
            }

            /*
             * 9. Danh sách năm có dữ liệu lịch đặt.
             */

            var cacNgayChamSoc =
                await _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.NgayChamSoc.HasValue)
                    .Select(x =>
                        x.NgayChamSoc!.Value)
                    .ToListAsync();

            var danhSachNam =
                cacNgayChamSoc
                    .Select(x =>
                        x.Year)
                    .Distinct()
                    .OrderByDescending(x =>
                        x)
                    .ToList();

            if (!danhSachNam.Contains(
                    DateTime.Today.Year))
            {
                danhSachNam.Insert(
                    0,
                    DateTime.Today.Year);
            }

            if (!danhSachNam.Contains(
                    namDangChon))
            {
                danhSachNam.Add(
                    namDangChon);

                danhSachNam =
                    danhSachNam
                        .OrderByDescending(x =>
                            x)
                        .ToList();
            }

            /*
             * 10. Thống kê tháng hiện tại.
             */

            int lichTrongThang =
                await _context.DatLichs
                    .AsNoTracking()
                    .CountAsync(x =>
                        x.NgayChamSoc.HasValue
                        &&
                        x.NgayChamSoc.Value
                            >= dauThangHienTai
                        &&
                        x.NgayChamSoc.Value
                            < dauThangHienTaiSau);

            decimal giaTriTrongThang =
                await _context.DatLichs
                    .AsNoTracking()
                    .Where(x =>
                        x.NgayChamSoc.HasValue
                        &&
                        x.NgayChamSoc.Value
                            >= dauThangHienTai
                        &&
                        x.NgayChamSoc.Value
                            < dauThangHienTaiSau
                        &&
                        x.TrangThai
                            == "Đã hoàn thành")
                    .SumAsync(x =>
                        x.TongTien ?? 0m);

            /*
             * 11. Tạo ViewModel trả về View.
             */
            /*
 * 12. Thống kê đánh giá khách hàng.
 */

            var danhGiaTrongPhamVi =
                await (
                    from danhGia in _context.DanhGias
                        .AsNoTracking()

                    join lich in _context.DatLichs
                        .AsNoTracking()
                        on danhGia.MaDatLich
                        equals lich.MaDatLich

                    where danhGia.NgayDanhGia
                              >= ngayBatDauPhamVi
                          &&
                          danhGia.NgayDanhGia
                              < ngayKetThucPhamVi

                    select new
                    {
                        danhGia.SoSao,

                        lich.MaNguoiChamSoc
                    })
                    .ToListAsync();

            int tongDanhGia =
                danhGiaTrongPhamVi.Count;

            double diemDanhGiaTrungBinh =
                tongDanhGia == 0
                    ? 0
                    : Math.Round(
                        danhGiaTrongPhamVi
                            .Average(x => x.SoSao),
                        1);

            int danhGiaTichCuc =
                danhGiaTrongPhamVi.Count(x =>
                    x.SoSao >= 4);

            int danhGiaCanChuY =
                danhGiaTrongPhamVi.Count(x =>
                    x.SoSao <= 2);

            var phanBoDanhGia =
                new List<AdminReportReviewStarViewModel>();

            for (int sao = 5;
                 sao >= 1;
                 sao--)
            {
                int soLuong =
                    danhGiaTrongPhamVi.Count(x =>
                        x.SoSao == sao);

                double tyLe =
                    tongDanhGia == 0
                        ? 0
                        : Math.Round(
                            soLuong
                            * 100.0
                            / tongDanhGia,
                            1);

                phanBoDanhGia.Add(
                    new AdminReportReviewStarViewModel
                    {
                        SoSao =
                            sao,

                        SoLuong =
                            soLuong,

                        TyLe =
                            tyLe
                    });
            }
            var topNguoiChamSoc =
    await (
        from danhGia in _context.DanhGias
            .AsNoTracking()

        join lich in _context.DatLichs
            .AsNoTracking()
            on danhGia.MaDatLich
            equals lich.MaDatLich

        join nguoiChamSoc
            in _context.NguoiChamSocs
                .AsNoTracking()
            on lich.MaNguoiChamSoc
            equals nguoiChamSoc.MaNguoiChamSoc

        where danhGia.NgayDanhGia
                  >= ngayBatDauPhamVi
              &&
              danhGia.NgayDanhGia
                  < ngayKetThucPhamVi

        group danhGia by new
        {
            nguoiChamSoc.MaNguoiChamSoc,
            nguoiChamSoc.HoTen
        }
        into nhom

        select new AdminTopCaregiverReviewViewModel
        {
            MaNguoiChamSoc =
                nhom.Key.MaNguoiChamSoc,

            HoTen =
                nhom.Key.HoTen,

            SoLuotDanhGia =
                nhom.Count(),

            DiemTrungBinh =
                nhom.Average(x =>
                    (double)x.SoSao)
        })
        .OrderByDescending(x =>
            x.DiemTrungBinh)
        .ThenByDescending(x =>
            x.SoLuotDanhGia)
        .Take(5)
        .ToListAsync();

            foreach (var item in topNguoiChamSoc)
            {
                item.DiemTrungBinh =
                    Math.Round(
                        item.DiemTrungBinh,
                        1);
            }
            var model =
                new AdminReportViewModel
                {
                    TongDanhGia =
    tongDanhGia,

                    DiemDanhGiaTrungBinh =
    diemDanhGiaTrungBinh,

                    DanhGiaTichCuc =
    danhGiaTichCuc,

                    DanhGiaCanChuY =
    danhGiaCanChuY,

                    PhanBoDanhGia =
    phanBoDanhGia,

                    TopNguoiChamSoc =
    topNguoiChamSoc,
                    NamDangChon =
                        namDangChon,

                    PhamVi =
                        phamVi,

                    DanhSachNam =
                        danhSachNam,

                    TongKhachHang =
                        await _context.KhachHangs
                            .AsNoTracking()
                            .CountAsync(),

                    TongNguoiChamSoc =
                        await _context.NguoiChamSocs
                            .AsNoTracking()
                            .CountAsync(),

                    TongLichDat =
                        await _context.DatLichs
                            .AsNoTracking()
                            .CountAsync(),

                    HoSoChoDuyet =
                        await _context.NguoiChamSocs
                            .AsNoTracking()
                            .CountAsync(x =>
                                x.TrangThai
                                == "Chờ duyệt"),

                    LichTrongThang =
                        lichTrongThang,

                    GiaTriHoanThanhTrongThang =
                        giaTriTrongThang,

                    TyLeHoanThanh =
                        tyLeHoanThanh,

                    TyLeHuy =
                        tyLeHuy,

                    TongLuotDatTrongKy =
                        tongLuotDatTrongKy,

                    TongDoanhThuTrongKy =
                        tongDoanhThuTrongKy,

                    NguoiDungMoiTrongKy =
                        nguoiDungMoiTrongKy,

                    TangTruongTrungBinh =
                        tangTruongTrungBinh,

                    BaoCaoTheoThang =
                        baoCaoTheoThang,

                    PhanBoTrangThai =
                        phanBoTrangThai,

                    BaoCaoKhuVuc =
                        baoCaoKhuVuc,

                    BaoCaoDichVu =
                        baoCaoDichVu,

                    LichGanDay =
                        lichTrongPhamVi
                            .OrderByDescending(x =>
                                x.NgayChamSoc)
                            .ThenByDescending(x =>
                                x.GioBatDau)
                            .Take(8)
                            .ToList()
                };

            return View(model);
        }

        private static string LayKhuVucTuDiaChi(
            string? diaChi)
        {
            if (string.IsNullOrWhiteSpace(diaChi))
            {
                return "Chưa cập nhật";
            }

            string[] cacPhan =
                diaChi.Split(
                    ',',
                    StringSplitOptions.RemoveEmptyEntries);

            foreach (string phan in cacPhan)
            {
                string giaTri =
                    phan.Trim();

                if (giaTri.StartsWith(
                        "Quận ",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return giaTri;
                }

                if (giaTri.StartsWith(
                        "Huyện ",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return giaTri;
                }

                if (giaTri.Contains(
                        "Thủ Đức",
                        StringComparison.OrdinalIgnoreCase))
                {
                    return "TP. Thủ Đức";
                }
            }

            return "Khu vực khác";
        }
    }
}