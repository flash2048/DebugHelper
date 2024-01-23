using System;
using System.IO;

namespace DebugHelper.Utilities
{
    public class TemporaryFile : IDisposable
    {
        public TemporaryFile()
        {
            FileName = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("n"));
        }

        public string FileName { get; }

        public string ReadAllText()
        {
            return File.Exists(FileName) ? File.ReadAllText(FileName) : string.Empty;
        }

        public void Dispose()
        {
            if (File.Exists(FileName))
            {
                File.Delete(FileName);
            }
        }
    }
}
