namespace FinalProject.Services.Interfaces
{
    public interface IMailService
    {
        Task SendEmailAsync(MailRequestVM mailRequestVM);
    }
}
