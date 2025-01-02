using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLL
{
    public class ForwardService
    {
        private readonly AppointmentService _appointmentService;
        private readonly UserService _userService;

        public ForwardService(AppointmentService appointmentService, UserService userService)
        {
            _appointmentService = appointmentService ?? throw new ArgumentNullException(nameof(appointmentService));
            _userService = userService ?? throw new ArgumentNullException(nameof(userService));
        }

        public async Task<(string userName, string userEmail, Guid patientId, List<AppointmentDTO> upcomingAppointments, List<AppointmentDTO> pastAppointments)> GetWelcomeMessageAsync(string email)
        {
            var user = await _userService.GetUserByEmailOrUsernameAsync(email).ConfigureAwait(false);
            if (user == null)
            {
                throw new Exception("User not found. Please log in again.");
            }

            var patientId = user.UserId;
            var userName = user.Name ?? throw new InvalidOperationException("User name cannot be null.");
            var userEmail = user.Email ?? throw new InvalidOperationException("User email cannot be null.");

            var upcomingAppointments = (await _appointmentService.GetUpcomingAppointmentsAsync(patientId)).ToList();
            var pastAppointments = (await _appointmentService.GetPastAppointmentsAsync(patientId)).ToList();

            return (userName, userEmail, patientId, upcomingAppointments, pastAppointments);
        }
    }
}
