using SportsTelemetryEngine.Domain.Entities;

namespace SportsTelemetryEngine.Application.Services;

public record PossessionEstimationResult(
    string MatchId,
    long FrameIndex,
    string? ControllingTeamId,
    string? ControllingPlayerId,
    double ConfidenceScore
);

public interface ITelemetryProcessor
{
    PossessionEstimationResult EstimateFramePossession(TrackingFrame frame);
    Task ProcessBatchAsync(IEnumerable<TrackingFrame> frames, CancellationToken cancellationToken = default);
}

public class TelemetryProcessor : ITelemetryProcessor
{
    private const double ProximityThresholdMeters = 1.5;

    public PossessionEstimationResult EstimateFramePossession(TrackingFrame frame)
    {
        if (!frame.Ball.InPlay || frame.Players.Count == 0)
        {
            return new PossessionEstimationResult(frame.MatchId, frame.FrameIndex, null, null, 0.0);
        }

        var ballPos = frame.Ball.Position;
        var closestPlayer = frame.Players
            .Select(p => new
            {
                Player = p,
                Distance = Math.Sqrt(Math.Pow(p.Position.X - ballPos.X, 2) + Math.Pow(p.Position.Y - ballPos.Y, 2))
            })
            .OrderBy(p => p.Distance)
            .FirstOrDefault();

        if (closestPlayer == null || closestPlayer.Distance > ProximityThresholdMeters)
        {
            return new PossessionEstimationResult(frame.MatchId, frame.FrameIndex, "Contested / Neutral", null, 0.3);
        }

        var confidence = Math.Clamp(1.0 - (closestPlayer.Distance / ProximityThresholdMeters), 0.5, 0.99);

        return new PossessionEstimationResult(
            frame.MatchId,
            frame.FrameIndex,
            closestPlayer.Player.TeamId,
            closestPlayer.Player.PlayerId,
            confidence
        );
    }

    public Task ProcessBatchAsync(IEnumerable<TrackingFrame> frames, CancellationToken cancellationToken = default)
    {
        // High-throughput processing pipeline placeholder
        return Task.CompletedTask;
    }
}
