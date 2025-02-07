namespace ms_framevideo_app.src.application.ports
{
    public interface IStoragePort
    {
        /// <summary>
        /// Faz download de um arquivo do S3 e retorna o caminho local onde foi salvo.
        /// </summary>
        string DownloadChunkFromS3(string bucketName, string chunkFileName);

        /// <summary>
        /// Faz upload de um arquivo para o S3 e retorna a URL final.
        /// </summary>
        string UploadFileToS3(string bucketName, string filePath, string destinationKey);
    }
}
