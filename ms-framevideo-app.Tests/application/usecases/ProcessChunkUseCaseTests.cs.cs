using Moq;
using ms_framevideo_app.src.application.ports;
using ms_framevideo_app.src.application.usecases;
using ms_framevideo_app.src.domain.entities;

namespace ms_framevideo_app.Tests.application.usecases
{
    public class ProcessChunkUseCaseTests
    {
        private readonly Mock<IStoragePort> _storagePortMock;
        private readonly Mock<IFrameProcessorPort> _frameProcessorPortMock;
        private readonly Mock<ICachePort> _cachePortMock;
        private readonly Mock<IMessagePublisherPort> _messagePublisherPortMock;
        private readonly ProcessChunkUseCase _useCase;

        public ProcessChunkUseCaseTests()
        {
            _storagePortMock = new Mock<IStoragePort>();
            _frameProcessorPortMock = new Mock<IFrameProcessorPort>();
            _cachePortMock = new Mock<ICachePort>();
            _messagePublisherPortMock = new Mock<IMessagePublisherPort>();

            _useCase = new ProcessChunkUseCase(
                _storagePortMock.Object,
                _frameProcessorPortMock.Object,
                _cachePortMock.Object,
                _messagePublisherPortMock.Object);
        }

        [Fact]
        public void Execute_Should_ProcessChunkAndUploadZip_AndUpdateCache_AndPublishMessage()
        {
            // Arrange
            var chunk = new Chunk(chunkId: "123", videoId: "vid456", originalFileName: "video.mp4", durationInSeconds: 100);
            string bucketInput = "input-bucket";
            string bucketOutput = "output-bucket";
            string localFilePath = "/tmp/video.mp4";
            string localZipPath = "/tmp/video.zip";
            string s3ZipLocation = "s3://output-bucket/video.zip";

            _storagePortMock.Setup(s => s.DownloadChunkFromS3(bucketInput, chunk.OriginalFileName))
                .Returns(localFilePath);
            _frameProcessorPortMock.Setup(f => f.ProcessChunk(chunk, localFilePath))
                .Returns(localZipPath);
            _storagePortMock.Setup(s => s.UploadFileToS3(bucketOutput, localZipPath, $"{chunk.VideoId}_{chunk.ChunkId}.zip"))
                .Returns(s3ZipLocation);

            // Act
            _useCase.Execute(chunk, bucketInput, bucketOutput);

            // Assert
            _storagePortMock.Verify(s => s.DownloadChunkFromS3(bucketInput, chunk.OriginalFileName), Times.Once);
            _frameProcessorPortMock.Verify(f => f.ProcessChunk(chunk, localFilePath), Times.Once);
            _storagePortMock.Verify(s => s.UploadFileToS3(bucketOutput, localZipPath, $"{chunk.VideoId}_{chunk.ChunkId}.zip"), Times.Once);
            _cachePortMock.Verify(c => c.UpdateFrameMetadata(chunk.ChunkId, chunk.VideoId, chunk.DurationInSeconds, s3ZipLocation), Times.Once);
            _messagePublisherPortMock.Verify(m => m.PublishChunkProcessed(chunk.ChunkId, s3ZipLocation, chunk.VideoId), Times.Once);
        }

        [Fact]
        public void Execute_Should_ThrowException_WhenDownloadFails()
        {
            // Arrange
            var chunk = new Chunk(chunkId: "123", videoId: "vid456", originalFileName: "video.mp4", durationInSeconds: 100);
            string bucketInput = "input-bucket";
            string bucketOutput = "output-bucket";

            _storagePortMock.Setup(s => s.DownloadChunkFromS3(bucketInput, chunk.OriginalFileName))
                .Throws(new Exception("S3 download failed"));

            // Act & Assert
            var exception = Assert.Throws<Exception>(() => _useCase.Execute(chunk, bucketInput, bucketOutput));
            Assert.Equal("S3 download failed", exception.Message);

            _storagePortMock.Verify(s => s.DownloadChunkFromS3(bucketInput, chunk.OriginalFileName), Times.Once);
            _frameProcessorPortMock.Verify(f => f.ProcessChunk(It.IsAny<Chunk>(), It.IsAny<string>()), Times.Never);
            _storagePortMock.Verify(s => s.UploadFileToS3(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
            _cachePortMock.Verify(c => c.UpdateFrameMetadata(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            _messagePublisherPortMock.Verify(m => m.PublishChunkProcessed(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }
    }
}
