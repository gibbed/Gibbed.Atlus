using System.IO;

namespace Gibbed.Atlus.FileFormats
{
    public class ArchiveValidateError
    {
        public string Message;
        public long Position;

        public ArchiveValidateError(string message)
            : this(message, -1)
        {
        }

        public ArchiveValidateError(string message, long position)
        {
            this.Message = message;
            this.Position = position;
        }

        public ArchiveValidateError(string message, Stream stream)
            : this(message, stream.Position)
        {
        }
    }
}
