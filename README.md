# Sports Telemetry Engine

High-throughput, clean-architecture sports tracking and telemetry ingestion engine built on **.NET 8**. Designed for processing real-time match event telemetry, ball tracking data, and frame-by-frame pitch possession calculation.

## Architecture

This solution adheres to **Clean Architecture** and Domain-Driven Design (DDD) principles:

```
src/
├── Domain/          # Core domain models (TrackingFrame, PlayerTelemetry, Ball)
├── Application/     # Application logic, possession estimation algorithms & services
├── Infrastructure/  # Persistence, external data sources, message brokers
└── WebApi/          # Minimal API endpoints & telemetry ingestion handlers
```

## Key Capabilities

- **High-Throughput Ingestion**: Lightweight endpoints ready for streaming telemetry frames.
- **Frame-by-Frame Possession Estimation**: Geometrical spatial distance evaluation between ball trajectory and player proximities.
- **Extensible Architecture**: Ready for Redis Caching, RabbitMQ event streaming, and PostgreSQL historical querying.

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)

### Run Locally
```bash
cd src/WebApi
dotnet run
```

### Health Check
```bash
curl http://localhost:5000/
```

## License
MIT
