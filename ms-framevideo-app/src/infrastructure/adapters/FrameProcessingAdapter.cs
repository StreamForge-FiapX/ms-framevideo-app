// src/infrastructure/adapters/FrameProcessingAdapter.cs
using ms_framevideo_app.src.application.ports;
using ms_framevideo_app.src.domain.entities;
using ms_framevideo_app.src.domain.services;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace ms_framevideo_app.src.infrastructure.adapters
{
    /// <summary>
    /// Adaptador que integra a lógica de processamento de frames (usando FrameGenerationService).
    /// </summary>
    public class FrameProcessingAdapter : IFrameProcessorPort
    {
        private readonly FrameGenerationService _frameGenerationService;

        public FrameProcessingAdapter(FrameGenerationService frameGenerationService)
        {
            _frameGenerationService = frameGenerationService;
        }

        public string ProcessChunk(Chunk chunk, string localChunkPath)
        {
            // 1) Gerar frames
            var frames = _frameGenerationService.GenerateFrames(chunk, localChunkPath);

            // 2) Compactar em .zip
            string localZipPath = CreateZipFile(chunk, frames);

            return localZipPath;
        }

        private string CreateZipFile(Chunk chunk, List<ms_framevideo_app.src.domain.entities.Frame> frames)
        {
            // Exemplo de nome de arquivo .zip local
            string zipFilePath = Path.Combine(Path.GetTempPath(), $"{chunk.VideoId}_{chunk.ChunkId}.zip");

            if (File.Exists(zipFilePath))
            {
                File.Delete(zipFilePath);
            }

            using (var zipArchive = ZipFile.Open(zipFilePath, ZipArchiveMode.Create))
            {
                foreach (var frame in frames)
                {
                    // Aqui, teoricamente, o arquivo .jpg teria sido gerado.
                    // Para simplificar, vamos fingir que ele existe em frame.FilePath.

                    // Adiciona arquivo ao zip
                    zipArchive.CreateEntryFromFile(frame.FilePath, Path.GetFileName(frame.FilePath));
                }
            }

            return zipFilePath;
        }
    }
}
