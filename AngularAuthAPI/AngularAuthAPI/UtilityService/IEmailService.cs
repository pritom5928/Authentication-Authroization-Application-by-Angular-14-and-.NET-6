using AngularAuthAPI.Models;

namespace AngularAuthAPI.UtilityService
{
    public interface IEmailService
    {
        void SendMail(EmailModel emailModel);
    }
}
