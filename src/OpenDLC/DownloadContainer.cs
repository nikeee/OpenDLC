using System;
using System.Collections.ObjectModel;
using System.IO;

namespace OpenDLC
{
    public abstract partial class DownloadContainer<T> : Collection<T>
    {
        public void SaveToFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));
            using (var fs = File.OpenWrite(fileName))
                SaveToStream(fs);
        }

        public abstract void SaveToStream(Stream stream);
    }
}
