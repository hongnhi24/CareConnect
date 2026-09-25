using CareConnect.Data;
using CareConnect.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace CareConnect.Services
{
    public class CaregiverProfileCompletenessService
        : ICaregiverProfileCompletenessService
    {
        private readonly CareConnectDbContext _context;


        public CaregiverProfileCompletenessService(
            CareConnectDbContext context)
        {
            _context = context;
        }


        public async Task<CaregiverProfileCompletenessViewModel>
            KiemTraAsync(
                int maNguoiChamSoc)
        {
            var nguoiChamSoc =
                await _context.NguoiChamSocs
                    .AsNoTracking()
                    .FirstOrDefaultAsync(x =>
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc);


            if (nguoiChamSoc == null)
            {
                return new CaregiverProfileCompletenessViewModel
                {
                    HoSoDayDu = false,

                    TongSoMuc = 10,

                    SoMucDaHoanThanh = 0,

                    PhanTramHoanThanh = 0,

                    ThieuThongTin =
                    {
                        "Không tìm thấy hồ sơ người chăm sóc"
                    }
                };
            }


            var thieu =
                new List<string>();


            // =================================================
            // 1. HỌ TÊN
            // =================================================

            if (string.IsNullOrWhiteSpace(
                    nguoiChamSoc.HoTen))
            {
                thieu.Add("Họ và tên");
            }


            // =================================================
            // 2. SỐ ĐIỆN THOẠI
            // =================================================

            if (string.IsNullOrWhiteSpace(
                    nguoiChamSoc.SoDienThoai))
            {
                thieu.Add("Số điện thoại");
            }


            // =================================================
            // 3. EMAIL
            // =================================================

            if (string.IsNullOrWhiteSpace(
                    nguoiChamSoc.Email))
            {
                thieu.Add("Email");
            }


            // =================================================
            // 4. ĐỊA CHỈ
            // =================================================

            if (string.IsNullOrWhiteSpace(
                    nguoiChamSoc.DiaChi))
            {
                thieu.Add("Địa chỉ");
            }


            // =================================================
            // 5. CHUYÊN MÔN
            // =================================================

            if (string.IsNullOrWhiteSpace(
                    nguoiChamSoc.ChuyenMon))
            {
                thieu.Add("Chuyên môn");
            }


            // =================================================
            // 6. KHU VỰC HOẠT ĐỘNG
            // =================================================

            if (string.IsNullOrWhiteSpace(
                    nguoiChamSoc.KhuVucHoatDong))
            {
                thieu.Add("Khu vực hoạt động");
            }


            // =================================================
            // 7. GIỚI THIỆU
            // =================================================

            if (string.IsNullOrWhiteSpace(
                    nguoiChamSoc.GioiThieu))
            {
                thieu.Add("Giới thiệu bản thân");
            }


            // =================================================
            // 8. KINH NGHIỆM
            // =================================================

            if (!nguoiChamSoc.KinhNghiem.HasValue)
            {
                thieu.Add("Kinh nghiệm");
            }


            // =================================================
            // 9. GIÁ THEO GIỜ
            // =================================================

            if (!nguoiChamSoc.GiaTheoGio.HasValue
                ||
                nguoiChamSoc.GiaTheoGio.Value <= 0)
            {
                thieu.Add("Giá theo giờ");
            }


            // =================================================
            // 10. CHỨNG CHỈ
            // =================================================

            bool coChungChi =
                await _context.ChungChis
                    .AsNoTracking()
                    .AnyAsync(x =>
                        x.MaNguoiChamSoc
                            == maNguoiChamSoc);


            if (!coChungChi)
            {
                thieu.Add(
                    "Ít nhất một bằng cấp hoặc chứng chỉ");
            }


            const int tongSoMuc =
                10;


            int soMucDaHoanThanh =
                tongSoMuc
                - thieu.Count;


            int phanTram =
                (int)Math.Round(
                    soMucDaHoanThanh
                    * 100d
                    / tongSoMuc);


            return new CaregiverProfileCompletenessViewModel
            {
                HoSoDayDu =
                    thieu.Count == 0,

                TongSoMuc =
                    tongSoMuc,

                SoMucDaHoanThanh =
                    soMucDaHoanThanh,

                PhanTramHoanThanh =
                    phanTram,

                ThieuThongTin =
                    thieu
            };
        }
    }
}