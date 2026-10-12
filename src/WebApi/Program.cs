using SportsTelemetryEngine.Application.Services;
using SportsTelemetryEngine.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSingleton<ITelemetryProcessor, TelemetryProcessor>();
builder.Services.AddSingleton<ITurnoverDetector, TurnoverDetector>();

var app = builder.Build();

app.MapGet("/", () => Results.Ok(new
{
    Service = "Sports Telemetry Engine",
    Status = "Healthy",
    Version = "1.0.0",
    Timestamp = DateTimeOffset.UtcNow
}));

app.MapPost("/api/v1/telemetry/frame", (TrackingFrame frame, ITelemetryProcessor processor, ITurnoverDetector turnoverDetector) =>
{
    var possession = processor.EstimateFramePossession(frame);
    var turnover = turnoverDetector.DetectTurnover(frame, possession);
    
    return Results.Ok(new
    {
        Possession = possession,
        Turnover = turnover
    });
});

app.Run();
