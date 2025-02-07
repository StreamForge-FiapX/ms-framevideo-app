namespace ms_framevideo_app.src.application.ports
{
    public interface ICachePort
    {
        /// <summary>
        /// Atualiza informações no Redis sobre o chunk processado.
        /// </summary>
        void UpdateFrameMetadata(string chunkId, string videoId, int frameCount, string s3ZipLocation);
    }
}
