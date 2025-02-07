using ms_framevideo_app.src.application.ports;

namespace ms_framevideo_app.src.application.usecases
{
    /// <summary>
    /// Atualiza os metadados de frames no Redis.
    /// Exemplifica um caso de uso separado, caso seja acionado por outro contexto.
    /// </summary>
    public class UpdateFrameMetadataUseCase
    {
        private readonly ICachePort _cachePort;

        public UpdateFrameMetadataUseCase(ICachePort cachePort)
        {
            _cachePort = cachePort;
        }

        public void Execute(string chunkId, string videoId, int frameCount, string s3Location)
        {
            _cachePort.UpdateFrameMetadata(chunkId, videoId, frameCount, s3Location);
        }
    }
}
