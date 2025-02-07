using ms_framevideo_app.src.application.ports;
using ms_framevideo_app.src.domain.entities;

namespace ms_framevideo_app.src.application.usecases
{
    /// <summary>
    /// Orquestra o processo:
    /// 1) Download do chunk
    /// 2) Processamento (gerar frames e zip)
    /// 3) Upload do .zip para S3
    /// 4) Atualização do cache
    /// 5) Publicação de mensagem no RabbitMQ
    /// </summary>
    public class ProcessChunkUseCase
    {
        private readonly IStoragePort _storagePort;
        private readonly IFrameProcessorPort _frameProcessorPort;
        private readonly ICachePort _cachePort;
        private readonly IMessagePublisherPort _messagePublisherPort;

        public ProcessChunkUseCase(
            IStoragePort storagePort,
            IFrameProcessorPort frameProcessorPort,
            ICachePort cachePort,
            IMessagePublisherPort messagePublisherPort)
        {
            _storagePort = storagePort;
            _frameProcessorPort = frameProcessorPort;
            _cachePort = cachePort;
            _messagePublisherPort = messagePublisherPort;
        }

        public void Execute(Chunk chunk, string bucketInput, string bucketOutput)
        {
            // 1) Download do chunk
            string localFilePath = _storagePort.DownloadChunkFromS3(bucketInput, chunk.OriginalFileName);

            // 2) Processamento => gerar frames, zipar => retorna caminho local do .zip
            string localZipPath = _frameProcessorPort.ProcessChunk(chunk, localFilePath);

            // 3) Upload do .zip
            string zipFileName = $"{chunk.VideoId}_{chunk.ChunkId}.zip";
            string s3ZipLocation = _storagePort.UploadFileToS3(bucketOutput, localZipPath, zipFileName);

            // 4) Atualiza cache
            // Para efeito de exemplo, assumimos que a contagem de frames está embutida na string de retorno,
            // mas você pode extrair a quantidade real do adaptador ou do FrameGenerationService.
            int totalFrames = chunk.DurationInSeconds; // Exemplo simplificado
            _cachePort.UpdateFrameMetadata(chunk.ChunkId, chunk.VideoId, totalFrames, s3ZipLocation);

            // 5) Publica mensagem no RabbitMQ
            _messagePublisherPort.PublishChunkProcessed(chunk.ChunkId, s3ZipLocation, chunk.VideoId);
        }
    }
}
