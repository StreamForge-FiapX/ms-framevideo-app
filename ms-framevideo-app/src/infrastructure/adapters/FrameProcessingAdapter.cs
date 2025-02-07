using System.IO.Abstractions;
using ms_framevideo_app.src.application.ports;
using ms_framevideo_app.src.domain.entities;
using System.IO.Compression;
using ms_framevideo_app.src.domain.services;

namespace ms_framevideo_app.src.infrastructure.adapters
{
    public class FrameProcessingAdapter : IFrameProcessorPort
    {
        private readonly IFrameGenerationServicePort _frameGenerationService;
        private readonly IFileSystem _fileSystem;

        public FrameProcessingAdapter(IFrameGenerationServicePort frameGenerationService, IFileSystem fileSystem)
        {
            _frameGenerationService = frameGenerationService;
            _fileSystem = fileSystem;
        }

        public string ProcessChunk(Chunk chunk, string localChunkPath)
        {
            // 1) Generate frames
            var frames = _frameGenerationService.GenerateFrames(chunk, localChunkPath);

            // 2) Create ZIP file
            string localZipPath = CreateZipFile(chunk, frames);

            return localZipPath;
        }

        private string CreateZipFile(Chunk chunk, List<Frame> frames)
        {
            string zipFilePath = _fileSystem.Path.Combine(_fileSystem.Path.GetTempPath(), $"{chunk.VideoId}_{chunk.ChunkId}.zip");

            if (_fileSystem.File.Exists(zipFilePath))
            {
                _fileSystem.File.Delete(zipFilePath);
            }

            // Create a new ZIP archive using the file system abstraction
            using (var zipStream = _fileSystem.File.Open(zipFilePath, FileMode.CreateNew))
            using (var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                foreach (var frame in frames)
                {
                    // Ensure the frame file exists in the mock file system
                    if (!_fileSystem.File.Exists(frame.FilePath))
                    {
                        throw new FileNotFoundException($"Frame file not found: {frame.FilePath}");
                    }

                    // Read the frame file content
                    var fileContent = _fileSystem.File.ReadAllBytes(frame.FilePath);

                    // Create a new entry in the ZIP archive
                    var entry = zipArchive.CreateEntry(_fileSystem.Path.GetFileName(frame.FilePath));

                    // Write the frame file content to the ZIP entry
                    using (var entryStream = entry.Open())
                    {
                        entryStream.Write(fileContent, 0, fileContent.Length);
                    }
                }
            }

            return zipFilePath;
        }
    }
}
