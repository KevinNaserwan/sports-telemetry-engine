namespace SportsTelemetryEngine.Domain.Entities;

public enum TurnoverType
{
    Interception,
    TackleWon,
    LooseBallRegain,
    OutStatus
}

public record TurnoverEvent(
    string MatchId,
    long FrameIndex,
    TimeSpan MatchTime,
    string PreviousTeamId,
    string RegainingTeamId,
    string? RegainingPlayerId,
    Coordinate Location,
    TurnoverType Type,
    double DistanceMeters
);
