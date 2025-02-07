using ms_framevideo_app.src.domain.entities;

namespace ms_framevideo_app.Tests.domain.entities
{
    public class FrameTests
    {
        [Fact]
        public void Constructor_ShouldInitializePropertiesCorrectly()
        {
            // Arrange
            string expectedFrameId = Guid.NewGuid().ToString();
            int expectedSecond = 10;
            string expectedFilePath = "/tmp/frame_10.jpg";

            // Act
            var frame = new Frame(expectedFrameId, expectedSecond, expectedFilePath);

            // Assert
            Assert.Equal(expectedFrameId, frame.FrameId);
            Assert.Equal(expectedSecond, frame.Second);
            Assert.Equal(expectedFilePath, frame.FilePath);
        }
    }
}
