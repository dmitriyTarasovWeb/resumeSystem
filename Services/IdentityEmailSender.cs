using Microsoft.AspNetCore.Identity;
using Resend;
using resumeSystem.Domain;

namespace resumeSystem.Services;

public class IdentityEmailSender : IEmailSender<ApplicationUser>
{
    private readonly IResend _resend;

    public IdentityEmailSender(IResend resend)
    {
        _resend = resend;
    }

    public async Task SendConfirmationLinkAsync(
        ApplicationUser user,
        string email,
        string confirmationLink)
    {
        var message = new EmailMessage
        {
            From = "resumeSystem.noreply@tarasov-dmitriy.xyz",
            Subject = "Подтверждение email",
            HtmlBody = $"""
                <h2>Подтвердите ваш email</h2>
                <p>Спасибо за регистрацию!</p>
                <p>
                    <a href="{confirmationLink}">
                        Подтвердить email
                    </a>
                </p>
                """
        };

        message.To.Add(email);

        await _resend.EmailSendAsync(message);
    }

    public async Task SendPasswordResetCodeAsync(
        ApplicationUser user,
        string email,
        string resetCode)
    {
        var message = new EmailMessage
        {
            From = "resumeSystem.noreply@tarasov-dmitriy.xyz",
            Subject = "Код восстановления пароля",
            HtmlBody = $"""
                <h2>Восстановление пароля</h2>
                <p>Ваш код:</p>
                <h3>{resetCode}</h3>
                """
        };

        message.To.Add(email);

        await _resend.EmailSendAsync(message);
    }

    public async Task SendPasswordResetLinkAsync(
        ApplicationUser user,
        string email,
        string resetLink)
    {
        var message = new EmailMessage
        {
            From = "resumeSystem.noreply@tarasov-dmitriy.xyz",
            Subject = "Восстановление пароля",
            HtmlBody = $"""
                <h2>Восстановление пароля</h2>
                <p>
                    <a href="{resetLink}">
                        Сбросить пароль
                    </a>
                </p>
                """
        };

        message.To.Add(email);

        await _resend.EmailSendAsync(message);
    }
}