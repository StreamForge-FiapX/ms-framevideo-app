// src/domain/entities/Frame.cs
namespace ms_framevideo_app.src.domain.entities
{
    public class Frame
    {
        public string FrameId { get; private set; }
        public int Second { get; private set; }
        public string FilePath { get; private set; }

        public Frame(string frameId, int second, string filePath)
        {
            FrameId = frameId;
            Second = second;
            FilePath = filePath;
        }
    }
}
