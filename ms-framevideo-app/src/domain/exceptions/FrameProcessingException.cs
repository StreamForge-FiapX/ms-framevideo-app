using System;

namespace ms_framevideo_app.src.domain.exceptions
{
    public class FrameProcessingException : Exception
    {
        public FrameProcessingException(string message) : base(message) { }

        public FrameProcessingException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
