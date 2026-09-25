namespace CareConnect.Services
{
    public interface ICaregiverMatchingService
    {
        // Lấy toàn bộ danh sách phù hợp
        Task<CaregiverMatchingResponse>
            TimDanhSachAsync(
                CaregiverMatchRequest request,
                CancellationToken cancellationToken =
                    default);


        // Dùng khi khách chọn:
        // "Hệ thống tự động phân công"
        Task<CaregiverMatchItem?>
            TimNguoiTotNhatAsync(
                CaregiverMatchRequest request,
                CancellationToken cancellationToken =
                    default);


        // Dùng khi khách tự chọn caregiver.
        // Server kiểm tra người khách chọn
        // còn hợp lệ và còn trống hay không.
        Task<CaregiverMatchItem?>
            KiemTraLuaChonAsync(
                int maNguoiChamSoc,
                CaregiverMatchRequest request,
                CancellationToken cancellationToken =
                    default);
    }
}