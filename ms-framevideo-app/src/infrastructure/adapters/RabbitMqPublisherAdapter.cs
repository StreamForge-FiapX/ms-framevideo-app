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

        public async void PublishChunkProcessed(string chunkId, string s3Location, string videoId)
        {
            if (string.IsNullOrWhiteSpace(chunkId)) throw new ArgumentException("chunkId não pode ser nulo ou vazio.", nameof(chunkId));
            if (string.IsNullOrWhiteSpace(s3Location)) throw new ArgumentException("s3Location não pode ser nulo ou vazio.", nameof(s3Location));
            if (string.IsNullOrWhiteSpace(videoId)) throw new ArgumentException("videoId não pode ser nulo ou vazio.", nameof(videoId));

            // Exemplo de configuração via appsettings.json
            var rabbitHost = _configuration["RabbitMQ:Host"];
            var queueName = _configuration["RabbitMQ:QueueOut"]; // ex.: frame-chunk-process

            if (string.IsNullOrWhiteSpace(rabbitHost)) throw new InvalidOperationException("O host do RabbitMQ não está configurado.");
            if (string.IsNullOrWhiteSpace(queueName)) throw new InvalidOperationException("O nome da fila do RabbitMQ não está configurado.");


            try
            {
                var factory = new ConnectionFactory() { HostName = rabbitHost };
                using var connection = await factory.CreateConnectionAsync();
                using var channel = await connection.CreateChannelAsync();

                // Declarar a fila
                await channel.QueueDeclareAsync(
                    queue: queueName,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                // Preparar a mensagem
                string message = $"{{ \"chunkId\": \"{chunkId}\", \"videoId\": \"{videoId}\", \"s3Location\": \"{s3Location}\" }}";
                var body = Encoding.UTF8.GetBytes(message);

                var basicProperties = new BasicProperties
                {
                    Persistent = true,
                    DeliveryMode = DeliveryModes.Persistent
                };

                // Publicar a mensagem
                await channel.BasicPublishAsync(
                    exchange: "",
                    routingKey: queueName,
                    mandatory: true,
                    basicProperties,
                    body: body
                );

                // Logar sucesso
                _logger.LogInformation($"Mensagem publicada na fila '{queueName}': {message}");
            }
            catch (Exception ex)
            {
                // Logar erros
                _logger.LogError(ex, $"Ocorreu um erro ao publicar a mensagem na fila '{queueName}'.");
                throw;
            }
        }
    }
}
