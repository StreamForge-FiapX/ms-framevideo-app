using System;
using System.Collections.Generic;
using System.IO;
using Xunit;
using ms_framevideo_app.src.domain.entities;
using ms_framevideo_app.src.domain.exceptions;
using ms_framevideo_app.src.domain.services;

namespace ms_framevideo_app.Tests.domain.services
{
    public class FrameGenerationServiceTests
    {
        private readonly FrameGenerationService _frameGenerationService;

        public FrameGenerationServiceTests()
        {
            _frameGenerationService = new FrameGenerationService();
        }

        [Fact]
        public void GenerateFrames_ShouldReturnCorrectNumberOfFrames()
        {
            // Arrange
            var chunk = new Chunk(chunkId: "123", videoId: "vid456", originalFileName: "video.mp4", durationInSeconds: 5);
            string localChunkFilePath = "/tmp/video.mp4";

            // Act
            var frames = _frameGenerationService.GenerateFrames(chunk, localChunkFilePath);

            // Assert
            Assert.NotNull(frames);
            Assert.Equal(chunk.DurationInSeconds, frames.Count);

            // Cada frame deve ter um caminho de arquivo não vazio
            foreach (var frame in frames)
            {
                Assert.False(string.IsNullOrEmpty(frame.FilePath));
                Assert.Contains("frame_", frame.FilePath);
            }
        }

        [Fact]
        public void GenerateFrames_ShouldThrowFrameProcessingException_WhenExceptionOccurs()
        {
            // Arrange
            var chunk = new Chunk(chunkId: "123", videoId: "vid456", originalFileName: "video.mp4", durationInSeconds: 5);
            string invalidPath = null; // Simula um erro ao passar um caminho inválido

            // Act & Assert
            var exception = Assert.Throws<FrameProcessingException>(() => _frameGenerationService.GenerateFrames(chunk, invalidPath));
            Assert.Contains($"Erro ao gerar frames para o chunk {chunk.ChunkId}", exception.Message);
        }
    }
}
