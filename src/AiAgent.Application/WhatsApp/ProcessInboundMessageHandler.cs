using AiAgent.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AiAgent.Application.WhatsApp;

internal sealed class ProcessInboundMessageHandler(
    IConversationSessionStore sessionStore,
    ILogger<ProcessInboundMessageHandler> logger)
    : IRequestHandler<ProcessInboundMessageCommand>
{
    public async Task Handle(ProcessInboundMessageCommand command, CancellationToken ct)
    {
        logger.LogInformation(
            "Inbound message from {Sender} for tenant {TenantId}: {Message}",
            command.SenderPhoneNumber, command.TenantId, command.MessageText);

        await sessionStore.AppendTurnAsync(
            command.SenderPhoneNumber,
            command.TenantId,
            "user",
            command.MessageText,
            ct);

        // TODO Milestone 3: classify intent → RAG or SQL path → generate Arabic response
    }
}
