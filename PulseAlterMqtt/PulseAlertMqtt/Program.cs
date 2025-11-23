using System;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace SamarcoMqttSubscriber
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddHostedService<MqttSubscriberService>();
                })
                .Build();

            await host.RunAsync();
        }
    }

    public class MqttSubscriberService : BackgroundService
    {
        private readonly string _mqttBroker;
        private readonly string _mqttTopic;
        private readonly int _mqttPort;
        private readonly string _mqttUser;
        private readonly string _mqttPassword;

        private IMqttClient _mqttClient;

        public MqttSubscriberService()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            _mqttBroker = config["MQTT:Broker"];
            _mqttTopic = config["MQTT:Topic"];
            _mqttPort = int.Parse(config["MQTT:Port"] ?? "1883");
            _mqttUser = config["MQTT:Username"];
            _mqttPassword = config["MQTT:Password"];
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _mqttClient = await ConnectToMqttAsync(stoppingToken);

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                string payload = Encoding.UTF8.GetString(e.ApplicationMessage.PayloadSegment);
                Console.WriteLine($"[MQTT RECEIVED] Topic: {e.ApplicationMessage.Topic} | Payload: {payload}");
                await Task.CompletedTask;
            };

            await _mqttClient.SubscribeAsync(new MqttTopicFilterBuilder()
                .WithTopic(_mqttTopic)
                .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                .Build(), stoppingToken);

            Console.WriteLine($"Subscribed to MQTT topic: {_mqttTopic}");

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, stoppingToken);
                }
            }
            catch (TaskCanceledException) { }
        }

        private async Task<IMqttClient> ConnectToMqttAsync(CancellationToken stoppingToken)
        {
            var factory = new MqttFactory();
            var client = factory.CreateMqttClient();

            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithTcpServer(_mqttBroker, _mqttPort)
                .WithCleanStart(true);

            if (!string.IsNullOrEmpty(_mqttUser) && !string.IsNullOrEmpty(_mqttPassword))
            {
                optionsBuilder.WithCredentials(_mqttUser, _mqttPassword);
            }

            // optionsBuilder.WithTls();

            var options = optionsBuilder.Build();

            client.DisconnectedAsync += async e =>
            {
                Console.WriteLine("Disconnected from MQTT broker. Trying to reconnect in 5s...");
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

                try
                {
                    await client.ConnectAsync(options, stoppingToken);
                    Console.WriteLine("Reconnected to MQTT broker.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Failed to reconnect: {ex.Message}");
                }
            };

            await client.ConnectAsync(options, stoppingToken);
            Console.WriteLine("Connected to MQTT broker.");

            return client;
        }
    }
}
