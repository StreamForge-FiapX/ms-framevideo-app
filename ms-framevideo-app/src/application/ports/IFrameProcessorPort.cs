// src/application/ports/IFrameProcessorPort.cs
using ms_framevideo_app.src.domain.entities;
using System.Collections.Generic;

namespace ms_framevideo_app.src.application.ports
{
    public interface IFrameProcessorPort
    {
        /// <summary>
        /// Processa um chunk de vídeo, gerando frames e criando o arquivo .zip
        /// Retorna o caminho local do arquivo .zip gerado.
        /// </summary>
        string ProcessChunk(Chunk chunk, string localChunkPath);
    }
}
