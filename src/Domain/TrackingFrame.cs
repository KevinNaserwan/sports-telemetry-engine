namespace SportsTelemetryEngine.Domain.Entities;

public record Coordinate(double X, double Y, double? Z = null);

public class PlayerTelemetry
{
    public required string PlayerId { get; init; }
    public required string TeamId { get; init; }
    public int JerseyNumber { get; init; }
    public Coordinate Position { get; set; } = new(0, 0);
    public double SpeedMps { get; set; }
    public double Acceleration { get; set; }
}

public class BallTelemetry
{
    public Coordinate Position { get; set; } = new(0, 0);
    public double SpeedMps { get; set; }
    public bool InPlay { get; set; } = true;
}

public class TrackingFrame
{
    public required string MatchId { get; init; }
    public long FrameIndex { get; init; }
    public TimeSpan MatchTime { get; init; }
    public BallTelemetry Ball { get; set; } = new();
    public List<PlayerTelemetry> Players { get; set; } = [];
    public DateTimeOffset TimestampUtc { get; init; } = DateTimeOffset.UtcNow;
}
