using ms_framevideo_app.src.application.usecases;
using ms_framevideo_app.src.domain.entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json.Linq;


namespace ms_framevideo_app.src.infrastructure.framework
{
    public class KubernetesWorkerService : BackgroundService
    {
        private readonly ILogger<KubernetesWorkerService> _logger;
        private readonly ProcessChunkUseCase _processChunkUseCase;
        private readonly IConfiguration _configuration;
        private IConnection? _connection;
        private IChannel? _channel;

        public KubernetesWorkerService(
            ILogger<KubernetesWorkerService> logger,
            ProcessChunkUseCase processChunkUseCase,
            IConfiguration configuration)
        {
            _logger = logger;
            _processChunkUseCase = processChunkUseCase;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await SetupRabbitMQAsync();
        }

        private async Task SetupRabbitMQAsync()
        {
            try
            {
                // Configuração via appsettings.json
                var rabbitHost = _configuration["RabbitMQ:Host"];
                var queueIn = _configuration["RabbitMQ:QueueIn"];

                if (string.IsNullOrWhiteSpace(rabbitHost)) throw new InvalidOperationException("RabbitMQ:Host não está configurado.");
                if (string.IsNullOrWhiteSpace(queueIn)) throw new InvalidOperationException("RabbitMQ:QueueIn não está configurado.");

                var factory = new ConnectionFactory() { HostName = rabbitHost };
                
                // Criação assíncrona da conexão e do canal
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                // Declarar a fila de entrada
                await _channel.QueueDeclareAsync(
                    queue: queueIn,
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null
                );

                var consumer = new AsyncEventingBasicConsumer(_channel);
                consumer.ReceivedAsync += async (model, ea) =>
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

                        // Ajuste o nome dos buckets conforme seu AppSettings ou Config
                        string bucketInput = "uploaded-video-chunk";
                        string bucketOutput = "uploaded-chunk-frame-bucket";

                        // Executar caso de uso
                        _processChunkUseCase.Execute(chunk, bucketInput, bucketOutput);

                        // Confirmação da mensagem processada com sucesso
                        await _channel.BasicAckAsync(ea.DeliveryTag, false);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar mensagem RabbitMQ.");

                        // Rejeitar a mensagem sem requeue
                        await _channel.BasicNackAsync(ea.DeliveryTag, false, false);
                    }
                };

                // Consumir mensagens da fila
                await _channel.BasicConsumeAsync(
                    queue: queueIn,
                    autoAck: false,
                    consumer: consumer
                );

                _logger.LogInformation("RabbitMQ configurado e aguardando mensagens...");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao configurar conexão RabbitMQ.");
                throw;
            }
        }

        public override async void Dispose()
        {
            try
            {
                if (_channel != null)
                {
                    await _channel.CloseAsync();
                    await _channel.DisposeAsync();
                }

                if (_connection != null)
                {
                    await _connection.CloseAsync();
                    await _connection.DisposeAsync();
                }
            }
            finally
            {
                GC.SuppressFinalize(this);
                base.Dispose();
            }
        }
    }
}
