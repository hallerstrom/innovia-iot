using MQTTnet;
using MQTTnet.Client;
using System.Text.Json;
using System.Text;

var factory = new MqttFactory();
var client = factory.CreateMqttClient();

var options = new MqttClientOptionsBuilder()
    .WithTcpServer("localhost", 1883)
    .Build();

Console.WriteLine("Edge.Simulator starting… connecting to MQTT at localhost:1883");
try
{
    await client.ConnectAsync(options);
    Console.WriteLine("✅ Connected to MQTT broker.");
}
catch (Exception ex)
{
    Console.WriteLine($"❌ Failed to connect to MQTT broker: {ex.Message}");
    throw;
}

// 👇 Lista över dina devices (serial 1–5)
var devices = new[]
{
    new { serial = "1", model = "TempIoT" },
    new { serial = "2", model = "TempIoT" },
    new { serial = "3", model = "TempIoT" },
    new { serial = "4", model = "TempIoT" },
    new { serial = "5", model = "TempIoT" },

    new { serial = "6", model = "CO2IoT" },
    new { serial = "7", model = "CO2IoT" },
    new { serial = "8", model = "CO2IoT" },
    new { serial = "9", model = "CO2IoT" },
    new { serial = "0", model = "CO2IoT" },
};

var rand = new Random();

while (true)
{
    foreach (var device in devices)
    {
        object[] metrics;

            if (device.model == "TempIoT")
            {
                metrics = new object[]
                {
                    new { type = "temperature", value = 20.0 + rand.NextDouble() * 4, unit = "C" }
                };
            }
            else
            {
                metrics = new object[]
                {
                    new { type = "co2", value = 700 + rand.Next(0, 800), unit = "ppm" }
                };
            }

        var payload = new
        {
            deviceId = device.serial, 
            apiKey = $"{device.serial}-key",
            timestamp = DateTimeOffset.UtcNow,
            metrics 
        };

        var topic = $"tenants/Innovia/devices/{device.serial}/measurements"; 
        var json = JsonSerializer.Serialize(payload);

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(Encoding.UTF8.GetBytes(json))
            .Build();

        await client.PublishAsync(message);

        Console.WriteLine($"[{DateTimeOffset.UtcNow:o}] Published to '{topic}': {json}");
    }

    // Skicka ny uppsättning värden var 10:e sekund
    await Task.Delay(TimeSpan.FromSeconds(10));
}
