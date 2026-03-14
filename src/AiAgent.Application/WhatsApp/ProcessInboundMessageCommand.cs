using MediatR;

namespace AiAgent.Application.WhatsApp;

public sealed record ProcessInboundMessageCommand(
    Guid TenantId,
    string SenderPhoneNumber,
    string MessageText,
    string FromPhoneNumberId,
    string DecryptedAccessToken
) : IRequest;
