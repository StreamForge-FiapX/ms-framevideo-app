// src/domain/entities/Chunk.cs
namespace ms_framevideo_app.src.domain.entities
{
    public class Chunk
    {
        public string ChunkId { get; set; }
        public string VideoId { get; set; }
        public string OriginalFileName { get; set; }
        public int DurationInSeconds { get; set; }

        public Chunk(string chunkId, string videoId, string originalFileName, int durationInSeconds)
        {
            ChunkId = chunkId;
            VideoId = videoId;
            OriginalFileName = originalFileName;
            DurationInSeconds = durationInSeconds;
        }
    }
}
