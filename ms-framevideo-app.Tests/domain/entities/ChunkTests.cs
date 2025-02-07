using Xunit;
using ms_framevideo_app.src.domain.entities;

namespace ms_framevideo_app.Tests.domain.entities
{
    public class ChunkTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            string expectedChunkId = "chunk123";
            string expectedVideoId = "video456";
            string expectedOriginalFileName = "sample.mp4";
            int expectedDuration = 120;

            // Act
            var chunk = new Chunk(expectedChunkId, expectedVideoId, expectedOriginalFileName, expectedDuration);

            // Assert
            Assert.Equal(expectedChunkId, chunk.ChunkId);
            Assert.Equal(expectedVideoId, chunk.VideoId);
            Assert.Equal(expectedOriginalFileName, chunk.OriginalFileName);
            Assert.Equal(expectedDuration, chunk.DurationInSeconds);
        }

        [Fact]
        public void Properties_ShouldBeModifiable()
        {
            // Arrange
            var chunk = new Chunk("chunk1", "video1", "file.mp4", 60)
            {
                // Act
                ChunkId = "newChunkId",
                VideoId = "newVideoId",
                OriginalFileName = "newFile.mp4",
                DurationInSeconds = 180
            };

            // Assert
            Assert.Equal("newChunkId", chunk.ChunkId);
            Assert.Equal("newVideoId", chunk.VideoId);
            Assert.Equal("newFile.mp4", chunk.OriginalFileName);
            Assert.Equal(180, chunk.DurationInSeconds);
        }
    }
}
