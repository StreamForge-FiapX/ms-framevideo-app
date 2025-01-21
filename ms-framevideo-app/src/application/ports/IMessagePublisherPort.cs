// src/application/ports/IMessagePublisherPort.cs
namespace ms_framevideo_app.src.application.ports
{
    public interface IMessagePublisherPort
    {
        /// <summary>
        /// Publica uma mensagem de que um chunk foi processado.
        /// </summary>
        void PublishChunkProcessed(string chunkId, string s3Location, string videoId);
    }
}
