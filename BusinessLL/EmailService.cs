using System.Net;
using System.Net.Mail;

namespace BusinessLL
{
    public class EmailService
    {
        private readonly string _gmailAddress;
        private readonly string _gmailPassword;

        public EmailService(string gmailAddress, string gmailPassword)
        {
            _gmailAddress = gmailAddress;
            _gmailPassword = gmailPassword;
        }

        /// <summary>
        /// Sends an email to the specified recipient.
        /// </summary>
        /// <param name="recipientEmail">The recipient's email address.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="body">The body content of the email (supports HTML).</param>
        public void SendEmail(string recipientEmail, string subject, string body)
        {
            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(_gmailAddress, _gmailPassword);
                    smtp.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_gmailAddress),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true // Allows for HTML content in the email body
                    };

                    mailMessage.To.Add(recipientEmail);

                    smtp.Send(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw new Exception("Email sending failed. Please check your email configuration.", ex);
            }
        }

        public async Task SendEmailAsync(string recipientEmail, string subject, string body)
        {
            try
            {
                using (var smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.Credentials = new NetworkCredential(_gmailAddress, _gmailPassword);
                    smtp.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_gmailAddress),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(recipientEmail);

                    await smtp.SendMailAsync(mailMessage); // Asynchronous call
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send email: {ex.Message}");
                throw new Exception("Email sending failed. Please check your email configuration.", ex);
            }
        }

        /// <summary>
        /// Sends a welcome email to a new user.
        /// </summary>
        public void SendWelcomeEmail(string recipientEmail, string userName)
        {
            string subject = "Welcome to Our Service!";
            string body = $@"
                            <h1>Welcome, {userName}!</h1>
                            <p>Thank you for signing up with us. We're excited to have you on board.</p>
                            <p>If you have any questions, feel free to contact our support team.</p>
                            <p>Best regards,<br>Our Team</p>";

            SendEmail(recipientEmail, subject, body);
        }

        public async Task SendWelcomeEmailAsync(string recipientEmail, string userName)
        {
            string subject = "Welcome to Our Service!";
            string body = $@"
                            <h1>Welcome, {userName}!</h1>
                            <p>Thank you for signing up with us. We're excited to have you on board.</p>
                            <p>If you have any questions, feel free to contact our support team.</p>
                            <p>Best regards,<br>Our Team</p>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        /// <summary>
        /// Sends an appointment confirmation email to the user.
        /// </summary>
        public async Task SendAppointmentConfirmationEmailAsync(string recipientEmail, string userName, DateTime appointmentDate)
        {
            var utcOffset = TimeSpan.FromHours(0); // Vietnam time zone (UTC+7)
            string subject = "Appointment Confirmation";
            string body = $@"
                    <h1>Appointment Confirmed</h1>
                    <p>Dear {userName},</p>
                    <p>Your appointment has been successfully booked for 
                    {TimeZoneHelper.ConvertUtcToLocal(appointmentDate, utcOffset):dddd, MMMM dd, yyyy} at {TimeZoneHelper.ConvertUtcToLocal(appointmentDate, utcOffset):hh:mm tt}.</p>
                    <p>Thank you for choosing our service!</p>
                    <p>Best regards,<br>Our Team</p>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        /// <summary>
        /// Sends a password reset token to the user.
        /// </summary>
        public async Task SendResetTokenEmailAsync(string recipientEmail, string resetToken)
        {
            var subject = "Password Reset Request";
            var body = $"Please use the following token to reset your password: {resetToken}";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        /// <summary>
        /// Sends an email to the user when an appointment is rescheduled.
        /// </summary>
        public async Task SendAppointmentRescheduledEmailAsync(string recipientEmail, string userName, string doctorName, DateTime newAppointmentDate)
        {
            var utcOffset = TimeSpan.FromHours(0); // Vietnam time zone (UTC+7)
            string subject = "Appointment Rescheduled";
            string body = $@"
                    <h1>Appointment Rescheduled</h1>
                    <p>Dear {userName},</p>
                    <p>Your appointment with Dr. {doctorName} has been rescheduled to 
                    {TimeZoneHelper.ConvertUtcToLocal(newAppointmentDate, utcOffset):dddd, MMMM dd, yyyy} at {TimeZoneHelper.ConvertUtcToLocal(newAppointmentDate, utcOffset):hh:mm tt}.</p>
                    <p>Thank you for your understanding!</p>
                    <p>Best regards,<br>Our Team</p>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        /// <summary>
        /// Sends an email to the user when an appointment is canceled.
        /// </summary>
        public async Task SendAppointmentCanceledEmailAsync(string recipientEmail, string userName, DateTime appointmentDate)
        {
            var utcOffset = TimeSpan.FromHours(0); // Vietnam time zone (UTC+7)
            string subject = "Appointment Canceled";
            string body = $@"
                    <h1>Appointment Canceled</h1>
                    <p>Dear {userName},</p>
                    <p>We regret to inform you that your appointment scheduled for 
                    {TimeZoneHelper.ConvertUtcToLocal(appointmentDate, utcOffset):dddd, MMMM dd, yyyy} at {TimeZoneHelper.ConvertUtcToLocal(appointmentDate, utcOffset):hh:mm tt} has been canceled.</p>
                    <p>We apologize for any inconvenience this may cause.</p>
                    <p>Best regards,<br>Our Team</p>";

            await SendEmailAsync(recipientEmail, subject, body);
        }

        /// <summary>
        /// Sends an email to the user when the status of an appointment is changed.
        /// </summary>
        public async Task SendAppointmentStatusChangedEmailAsync(string recipientEmail, string userName, DateTime appointmentDate, string newStatus)
        {
            var utcOffset = TimeSpan.FromHours(0); // Vietnam time zone (UTC+7)
            string subject = "Appointment Status Changed";
            string body = $@"
                    <h1>Appointment Status Changed</h1>
                    <p>Dear {userName},</p>
                    <p>The status of your appointment scheduled for 
                    {TimeZoneHelper.ConvertUtcToLocal(appointmentDate, utcOffset):dddd, MMMM dd, yyyy} at {TimeZoneHelper.ConvertUtcToLocal(appointmentDate, utcOffset):hh:mm tt} has been changed to {newStatus}.</p>
                    <p>Thank you for your attention!</p>
                    <p>Best regards,<br>Our Team</p>";

            await SendEmailAsync(recipientEmail, subject, body);
        }
    }
}
