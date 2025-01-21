// src/infrastructure/framework/KubernetesWorkerService.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ms_framevideo_app.src.application.usecases;
using ms_framevideo_app.src.domain.entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore.Metadata;


namespace ms_framevideo_app.src.infrastructure.framework
{
    public class KubernetesWorkerService : BackgroundService
    {
        private readonly ILogger<KubernetesWorkerService> _logger;
        private readonly ProcessChunkUseCase _processChunkUseCase;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IModel? _channel;

        public KubernetesWorkerService(
            ILogger<KubernetesWorkerService> logger,
            ProcessChunkUseCase processChunkUseCase,
            IConfiguration configuration)
        {
            _logger = logger;
            _processChunkUseCase = processChunkUseCase;
            _configuration = configuration;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            SetupRabbitMQ();
            return Task.CompletedTask;
        }

        private void SetupRabbitMQ()
        {
            try
            {
                var rabbitHost = _configuration["RabbitMQ:Host"];
                var queueIn = _configuration["RabbitMQ:QueueIn"];

                var factory = new ConnectionFactory() { HostName = rabbitHost };
                _connection = (IConnection?)factory.CreateConnectionAsync();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                    queue: queueIn,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    _logger.LogInformation($"Mensagem recebida da fila {queueIn}: {message}");

                    try
                    {
                        // Exemplo de parsing JSON
                        var json = JObject.Parse(message);
                        string chunkId = json["chunkId"]?.ToString() ?? Guid.NewGuid().ToString();
                        string videoId = json["videoId"]?.ToString() ?? "unknown";
                        string fileName = json["fileName"]?.ToString() ?? "chunk.mp4";
                        int durationInSeconds = json["duration"]?.ToObject<int>() ?? 60;

                        // Monta objeto Chunk
                        var chunk = new Chunk(chunkId, videoId, fileName, durationInSeconds);

                        // Chama caso de uso
                        // Ajuste o nome dos buckets conforme seu AppSettings ou Config
                        string bucketInput = "uploaded-video-chunk";
                        string bucketOutput = "uploaded-chunk-frame-bucket";

                        _processChunkUseCase.Execute(chunk, bucketInput, bucketOutput);

                        _channel.BasicAck(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem RabbitMQ.");
                        // Podemos fazer requeue ou descartar a mensagem
                        _channel.BasicNack(ea.DeliveryTag, false, false);
                    }
                };

                _channel.BasicConsume(
                    queue: queueIn,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("KubernetesWorkerService iniciado e aguardando mensagens...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao configurar conexão RabbitMQ");
            }
        }

        public override void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
            base.Dispose();
        }
    }
}
