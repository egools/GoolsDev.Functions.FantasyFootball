using System;

namespace GoolsDev.Functions.FantasyFootball
{
    public record FailedEspnCallDocument(
        Guid Id,
        string GameId,
        object Request,
        string Response);
}
