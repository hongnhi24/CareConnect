using CareConnect.ViewModels;

namespace CareConnect.Services
{
    public interface ICaregiverProfileCompletenessService
    {
        Task<CaregiverProfileCompletenessViewModel>
            KiemTraAsync(int maNguoiChamSoc);
    }
}