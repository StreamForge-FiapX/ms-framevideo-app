// src/infrastructure/adapters/RedisCacheAdapter.cs
using ms_framevideo_app.src.application.ports;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace ms_framevideo_app.src.infrastructure.adapters
{
    public class RedisCacheAdapter : ICachePort
    {
        private readonly ILogger<RedisCacheAdapter> _logger;
        private readonly IConfiguration _configuration;
        private readonly ConnectionMultiplexer _redisConnection;

        public RedisCacheAdapter(ILogger<RedisCacheAdapter> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;

            // Exemplo: "Redis:ConnectionString": "my-redis-cluster.abc123.ng.0001.use1.cache.amazonaws.com:6379"
            string redisConnectionString = _configuration["Redis:ConnectionString"];
            _redisConnection = ConnectionMultiplexer.Connect(redisConnectionString);
        }

        public void UpdateFrameMetadata(string chunkId, string videoId, int frameCount, string s3ZipLocation)
        {
            var db = _redisConnection.GetDatabase();

            // Exemplo de chave no Redis: "frames:chunkId"
            string key = $"frames:{chunkId}";

            db.HashSet(key, new[]
            {
                new HashEntry("VideoId", videoId),
                new HashEntry("FrameCount", frameCount),
                new HashEntry("ZipLocation", s3ZipLocation)
            });

            _logger.LogInformation($"Dados do chunk {chunkId} atualizados no Redis.");
        }
    }
}
