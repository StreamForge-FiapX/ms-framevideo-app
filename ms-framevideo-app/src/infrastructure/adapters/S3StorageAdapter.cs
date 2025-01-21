// src/infrastructure/adapters/S3StorageAdapter.cs
using ms_framevideo_app.src.application.ports;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ms_framevideo_app.src.infrastructure.adapters
{
    public class S3StorageAdapter : IStoragePort
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<S3StorageAdapter> _logger;
        private readonly IConfiguration _configuration;

        public S3StorageAdapter(IAmazonS3 s3Client, ILogger<S3StorageAdapter> logger, IConfiguration configuration)
        {
            _s3Client = s3Client;
            _logger = logger;
            _configuration = configuration;
        }

        public string DownloadChunkFromS3(string bucketName, string chunkFileName)
        {
            // Cria caminho local temporário
            string localFilePath = Path.Combine(Path.GetTempPath(), chunkFileName);

            try
            {
                var response = _s3Client.GetObjectAsync(bucketName, chunkFileName).Result;
                using (var responseStream = response.ResponseStream)
                using (var fs = new FileStream(localFilePath, FileMode.Create, FileAccess.Write))
                {
                    responseStream.CopyTo(fs);
                }

                _logger.LogInformation($"Download do arquivo {chunkFileName} concluído. Local: {localFilePath}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao fazer download do arquivo {chunkFileName}: {ex.Message}");
                throw;
            }

            return localFilePath;
        }

        public string UploadFileToS3(string bucketName, string filePath, string destinationKey)
        {
            try
            {
                var putRequest = new PutObjectRequest
                {
                    BucketName = bucketName,
                    FilePath = filePath,
                    Key = destinationKey
                };

                PutObjectResponse response = _s3Client.PutObjectAsync(putRequest).Result;

                _logger.LogInformation($"Upload do arquivo {filePath} concluído. Key: {destinationKey}");

                // Exemplo de retorno de URL
                return $"s3://{bucketName}/{destinationKey}";
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao fazer upload do arquivo {filePath}: {ex.Message}");
                throw;
            }
        }
    }
}
