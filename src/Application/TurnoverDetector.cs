using SportsTelemetryEngine.Domain.Entities;

namespace SportsTelemetryEngine.Application.Services;

public interface ITurnoverDetector
{
    TurnoverEvent? DetectTurnover(TrackingFrame frame, PossessionEstimationResult currentPossession);
    void Reset();
}

public class TurnoverDetector : ITurnoverDetector
{
    private string? _lastPossessingTeam;
    private long _lastPossessionFrame;
    private const int MinContestedHoldFrames = 3;
    private int _contestedCounter = 0;

    public void Reset()
    {
        _lastPossessingTeam = null;
        _lastPossessionFrame = 0;
        _contestedCounter = 0;
    }

    public TurnoverEvent? DetectTurnover(TrackingFrame frame, PossessionEstimationResult currentPossession)
    {
        var currentTeam = currentPossession.ControllingTeamId;

        if (string.IsNullOrEmpty(currentTeam) || currentTeam == "Contested / Neutral")
        {
            _contestedCounter++;
            return null;
        }

        // First registered possession
        if (string.IsNullOrEmpty(_lastPossessingTeam))
        {
            _lastPossessingTeam = currentTeam;
            _lastPossessionFrame = frame.FrameIndex;
            _contestedCounter = 0;
            return null;
        }

        // Turnover registered when possession flips to opposite team
        if (_lastPossessingTeam != currentTeam)
        {
            var turnoverType = _contestedCounter >= MinContestedHoldFrames
                ? TurnoverType.LooseBallRegain
                : TurnoverType.Interception;

            var prevTeam = _lastPossessingTeam;
            _lastPossessingTeam = currentTeam;
            _lastPossessionFrame = frame.FrameIndex;
            _contestedCounter = 0;

            return new TurnoverEvent(
                MatchId: frame.MatchId,
                FrameIndex: frame.FrameIndex,
                MatchTime: frame.MatchTime,
                PreviousTeamId: prevTeam,
                RegainingTeamId: currentTeam,
                RegainingPlayerId: currentPossession.ControllingPlayerId,
                Location: frame.Ball.Position,
                Type: turnoverType,
                DistanceMeters: 0.0
            );
        }

        _contestedCounter = 0;
        return null;
    }
}
