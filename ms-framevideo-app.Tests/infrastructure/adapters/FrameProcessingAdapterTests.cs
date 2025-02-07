using System.IO.Abstractions.TestingHelpers;
using Moq;
using ms_framevideo_app.src.domain.entities;
using ms_framevideo_app.src.domain.services;
using ms_framevideo_app.src.infrastructure.adapters;
using Xunit;

namespace ms_framevideo_app.Tests.infrastructure.adapters
{
    public class FrameProcessingAdapterTests
    {
        private readonly Mock<IFrameGenerationServicePort> _frameGenerationServiceMock;
        private readonly FrameProcessingAdapter _adapter;
        private readonly MockFileSystem _fileSystem;

        public FrameProcessingAdapterTests()
        {
            _frameGenerationServiceMock = new Mock<IFrameGenerationServicePort>();
            _fileSystem = new MockFileSystem();

            _adapter = new FrameProcessingAdapter(_frameGenerationServiceMock.Object, _fileSystem);
        }

        [Fact]
        public void ProcessChunk_Should_GenerateFrames_And_CreateZip()
        {
            // Arrange
            var chunk = new Chunk(chunkId: "123", videoId: "vid456", originalFileName: "video.mp4", durationInSeconds: 100);
            string localChunkPath = "/tmp/video.mp4";
            string expectedZipPath = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), $"{chunk.VideoId}_{chunk.ChunkId}.zip");

            var frames = new List<Frame>
            {
                new(frameId: "1", second: 10, filePath: "/tmp/frame1.jpg"),
                new(frameId: "2", second: 20, filePath: "/tmp/frame2.jpg")
            };

            _frameGenerationServiceMock.Setup(f => f.GenerateFrames(chunk, localChunkPath)).Returns(frames);
            _fileSystem.Directory.CreateDirectory("/tmp");

            // Criar arquivos simulados para os frames
            _fileSystem.AddFile("/tmp/frame1.jpg", new MockFileData("fake image data"));
            _fileSystem.AddFile("/tmp/frame2.jpg", new MockFileData("fake image data"));

            // Act
            string resultZipPath = _adapter.ProcessChunk(chunk, localChunkPath);

            // Assert
            _frameGenerationServiceMock.Verify(f => f.GenerateFrames(chunk, localChunkPath), Times.Once);
            Assert.Equal(expectedZipPath, resultZipPath);
            Assert.True(_fileSystem.File.Exists(resultZipPath)); // O arquivo .zip deve existir
        }
    }
}
