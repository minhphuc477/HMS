// File: ../BusinessLL/IAppointmentValidationService.cs

using System.Threading.Tasks;
using DataTransferO;

namespace BusinessLL
{
    public interface IAppointmentValidationService
    {
        Task ValidateAppointmentAsync(AppointmentDTO appointmentDto);
    }
}
