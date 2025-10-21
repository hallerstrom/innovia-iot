using Microsoft.AspNetCore.SignalR;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR();

builder.Services.AddCors(opt =>
{
    opt.AddPolicy("Frontend", policy =>
    {
        policy.WithOrigins(
                "http://127.0.0.1:5500",
                "http://localhost:5500",
                "http://127.0.0.1:5173",
                "http://localhost:5173"
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

var app = builder.Build();
app.UseCors("Frontend");
app.MapHub<TelemetryHub>("/hub/telemetry").RequireCors("Frontend");
app.Run();

public class TelemetryHub : Hub
{
    public Task JoinTenant(string tenant) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{tenant}");

    public async Task PublishMeasurement(RealtimeMeasurement m)
    {
        await Clients.Group($"tenant:{m.TenantSlug}")
            .SendAsync("measurementReceived", m);
    }
}

public record RealtimeMeasurement(
    string TenantSlug,
    System.Guid DeviceId,
    string Type,
    double Value,
    System.DateTimeOffset Time
);
