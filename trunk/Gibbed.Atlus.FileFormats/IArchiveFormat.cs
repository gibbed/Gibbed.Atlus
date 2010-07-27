using System.Collections.Generic;

namespace Gibbed.Atlus.FileFormats
{
    public interface IArchiveFormat
    {
        bool Validate(string path, out ArchiveValidateError error);
        List<ArchiveEntry> GetEntries(string path);
    }
}
