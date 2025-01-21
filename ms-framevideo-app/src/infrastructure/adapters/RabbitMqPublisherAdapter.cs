// src/infrastructure/adapters/RabbitMqPublisherAdapter.cs
using ms_framevideo_app.src.application.ports;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using System.Text;

namespace ms_framevideo_app.src.infrastructure.adapters
{
    public class RabbitMqPublisherAdapter : IMessagePublisherPort
    {
        private readonly ILogger<RabbitMqPublisherAdapter> _logger;
        private readonly IConfiguration _configuration;

        public RabbitMqPublisherAdapter(ILogger<RabbitMqPublisherAdapter> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public void PublishChunkProcessed(string chunkId, string s3Location, string videoId)
        {
            // Exemplo de configuração via appsettings.json
            var rabbitHost = _configuration["RabbitMQ:Host"];
            var queueName = _configuration["RabbitMQ:QueueOut"]; // ex.: frame-chunk-process

            var factory = new ConnectionFactory() { HostName = rabbitHost };

            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: queueName,
                                     durable: true,
                                     exclusive: false,
                                     autoDelete: false,
                                     arguments: null);

                string message = $"{{ \"chunkId\": \"{chunkId}\", \"videoId\":\"{videoId}\", \"s3Location\":\"{s3Location}\" }}";
                var body = Encoding.UTF8.GetBytes(message);

                channel.BasicPublish(exchange: "",
                                     routingKey: queueName,
                                     basicProperties: null,
                                     body: body);

                _logger.LogInformation($"Mensagem publicada para fila {queueName}: {message}");
            }
        }
    }
}
