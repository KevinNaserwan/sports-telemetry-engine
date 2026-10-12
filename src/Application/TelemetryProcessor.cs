using SportsTelemetryEngine.Domain.Entities;

namespace SportsTelemetryEngine.Application.Services;

public record BatchProcessResult(
    string MatchId,
    int TotalFramesProcessed,
    double HomePossessionPercentage,
    double AwayPossessionPercentage,
    double ContestedPercentage,
    int TotalTurnoversCount,
    List<TurnoverEvent> Turnovers
);

public interface ITelemetryProcessor
{
    PossessionEstimationResult EstimateFramePossession(TrackingFrame frame);
    BatchProcessResult ProcessBatch(IEnumerable<TrackingFrame> frames);
    Task<BatchProcessResult> ProcessBatchAsync(IEnumerable<TrackingFrame> frames, CancellationToken cancellationToken = default);
}

public class TelemetryProcessor : ITelemetryProcessor
{
    private const double ProximityThresholdMeters = 1.5;
    private readonly ITurnoverDetector _turnoverDetector;

    public TelemetryProcessor(ITurnoverDetector? turnoverDetector = null)
    {
        _turnoverDetector = turnoverDetector ?? new TurnoverDetector();
    }

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

    public BatchProcessResult ProcessBatch(IEnumerable<TrackingFrame> frames)
    {
        _turnoverDetector.Reset();
        var frameList = frames.ToList();
        if (frameList.Count == 0)
        {
            return new BatchProcessResult("UNKNOWN", 0, 0.0, 0.0, 0.0, 0, []);
        }

        var matchId = frameList.First().MatchId;
        int homeCount = 0;
        int awayCount = 0;
        int contestedCount = 0;
        var turnovers = new List<TurnoverEvent>();

        foreach (var frame in frameList)
        {
            var possession = EstimateFramePossession(frame);
            var turnover = _turnoverDetector.DetectTurnover(frame, possession);
            if (turnover != null)
            {
                turnovers.Add(turnover);
            }

            if (possession.ControllingTeamId == "Home" || (possession.ControllingTeamId?.StartsWith("H") ?? false))
            {
                homeCount++;
            }
            else if (possession.ControllingTeamId == "Away" || (possession.ControllingTeamId?.StartsWith("A") ?? false))
            {
                awayCount++;
            }
            else
            {
                contestedCount++;
            }
        }

        int total = frameList.Count;
        return new BatchProcessResult(
            MatchId: matchId,
            TotalFramesProcessed: total,
            HomePossessionPercentage: Math.Round((double)homeCount / total * 100.0, 2),
            AwayPossessionPercentage: Math.Round((double)awayCount / total * 100.0, 2),
            ContestedPercentage: Math.Round((double)contestedCount / total * 100.0, 2),
            TotalTurnoversCount: turnovers.Count,
            Turnovers: turnovers
        );
    }

    public Task<BatchProcessResult> ProcessBatchAsync(IEnumerable<TrackingFrame> frames, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(ProcessBatch(frames));
    }
}
