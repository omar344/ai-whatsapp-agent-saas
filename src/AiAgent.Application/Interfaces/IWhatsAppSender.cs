namespace AiAgent.Application.Interfaces;

public interface IWhatsAppSender
{
    Task SendTextMessageAsync(
        string toPhoneNumber,
        string fromPhoneNumberId,
        string decryptedAccessToken,
        string message,
        CancellationToken ct = default);
}
