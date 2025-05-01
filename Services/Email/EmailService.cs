// Services/Email/EmailService.cs
using System.Net;
using System.Net.Mail;
using AspnetCoreMvcFull.Models;

namespace AspnetCoreMvcFull.Services
{
  public class EmailService : IEmailService
  {
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;
    private readonly EmailTemplate _emailTemplate;
    private readonly string _env;

    public EmailService(
        IConfiguration configuration,
        ILogger<EmailService> logger,
        EmailTemplate emailTemplate)
    {
      _configuration = configuration;
      _logger = logger;
      _emailTemplate = emailTemplate;

      // Default environment adalah "development"
      _env = "development";
    }

    public async Task SendEmailAsync(string toEmail, string subject, string body)
    {
      var smtpSettings = _configuration.GetSection("SmtpSettings");
      string emailSent = toEmail;

      if (_env == "test")
      {
        emailSent = "edoarya2002@gmail.com"; // Email pengujian
      }

      var message = new MailMessage();
      message.From = new MailAddress(smtpSettings["SenderEmail"], smtpSettings["SenderName"]);
      message.To.Add(new MailAddress(emailSent));
      message.Subject = subject;
      message.Body = body;
      message.IsBodyHtml = true;

      using (var smtpClient = new SmtpClient(smtpSettings["Server"], int.Parse(smtpSettings["Port"])))
      {
        smtpClient.Credentials = new NetworkCredential(smtpSettings["Username"], smtpSettings["Password"]);
        smtpClient.EnableSsl = bool.Parse(smtpSettings["UseSsl"]);

        await smtpClient.SendMailAsync(message);
        _logger.LogInformation($"Email berhasil dikirim ke {emailSent}");
      }
    }

    public async Task SendBookingSubmittedEmailAsync(Booking booking, string userEmail)
    {
      var subject = $"Booking Crane #{booking.BookingNumber} - Berhasil Diajukan";
      var body = _emailTemplate.BookingSubmittedTemplate(booking.Name, booking);
      await SendEmailAsync(userEmail, subject, body);
    }

    public async Task SendManagerApprovalRequestEmailAsync(Booking booking, string managerEmail, string managerName, string badgeNumber)
    {
      var subject = $"Permintaan Persetujuan Booking Crane #{booking.BookingNumber}";
      var body = _emailTemplate.ManagerApprovalTemplate(managerName, booking, booking.Name, badgeNumber, 1);
      await SendEmailAsync(managerEmail, subject, body);
    }

    public async Task SendBookingManagerApprovedEmailAsync(Booking booking, string userEmail)
    {
      var subject = $"Booking Crane #{booking.BookingNumber} - Disetujui oleh Manager";
      var body = _emailTemplate.BookingManagerApprovedTemplate(booking.Name, booking);
      await SendEmailAsync(userEmail, subject, body);
    }

    public async Task SendPicApprovalRequestEmailAsync(Booking booking, string picEmail, string picName, string badgeNumber)
    {
      var subject = $"Permintaan Persetujuan Booking Crane #{booking.BookingNumber}";
      var body = _emailTemplate.PicApprovalTemplate(picName, booking, booking.Name, badgeNumber, 2);
      await SendEmailAsync(picEmail, subject, body);
    }

    public async Task SendBookingApprovedEmailAsync(Booking booking, string userEmail)
    {
      var subject = $"Booking Crane #{booking.BookingNumber} - Disetujui Sepenuhnya";
      var body = _emailTemplate.BookingApprovedTemplate(booking.Name, booking);
      await SendEmailAsync(userEmail, subject, body);
    }

    public async Task SendBookingRejectedEmailAsync(Booking booking, string userEmail, string rejectorName, string rejectReason)
    {
      var subject = $"Booking Crane #{booking.BookingNumber} - Ditolak";
      var body = _emailTemplate.BookingRejectedTemplate(booking.Name, booking, rejectorName, rejectReason);
      await SendEmailAsync(userEmail, subject, body);
    }

    public async Task SendBookingCancelledEmailAsync(Booking booking, string userEmail, string cancelledBy, string cancelReason)
    {
      var subject = $"Booking Crane #{booking.BookingNumber} - Dibatalkan";
      var body = _emailTemplate.BookingCancelledTemplate(booking.Name, booking, cancelledBy, cancelReason);
      await SendEmailAsync(userEmail, subject, body);
    }

    public async Task SendBookingRevisedEmailAsync(Booking booking, string receiverEmail)
    {
      var subject = $"Booking Crane #{booking.BookingNumber} - Revisi Diajukan";
      var body = _emailTemplate.BookingRevisedTemplate(booking.Name, booking);
      await SendEmailAsync(receiverEmail, subject, body);
    }
  }
}
