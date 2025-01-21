// src/api/FrameProcessingController.cs
using Microsoft.AspNetCore.Mvc;
using ms_framevideo_app.src.application.usecases;
using ms_framevideo_app.src.domain.entities;

namespace ms_framevideo_app.src.api
{
    [ApiController]
    [Route("[controller]")]
    public class FrameProcessingController : ControllerBase
    {
        private readonly ProcessChunkUseCase _processChunkUseCase;

        public FrameProcessingController(ProcessChunkUseCase processChunkUseCase)
        {
            _processChunkUseCase = processChunkUseCase;
        }

        [HttpPost("process-chunk")]
        public IActionResult ProcessChunk([FromBody] Chunk chunk)
        {
            // Buckets podem vir de config ou podem ser passados no body
            string bucketInput = "uploaded-video-chunk";
            string bucketOutput = "uploaded-chunk-frame-bucket";

            _processChunkUseCase.Execute(chunk, bucketInput, bucketOutput);

            return Ok($"Chunk {chunk.ChunkId} processado com sucesso!");
        }
    }
}
