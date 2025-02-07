using ms_framevideo_app.src.domain.entities;

namespace ms_framevideo_app.src.domain.services
{
    public interface IFrameGenerationServicePort
    {
        public List<Frame> GenerateFrames(Chunk chunk, string localChunkFilePath);
    }
}
