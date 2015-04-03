using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace OpenDLC
{
    public abstract class DownloadContainer<T> : Collection<T>
    {
        public void SaveToFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName"); // TODO: nameof(fileName)

            using (var fs = File.OpenWrite(fileName))
                SaveToStream(fs);
        }
#if FEATURE_TAP
        public async Task SaveToFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName"); // TODO: nameof(fileName)

            using (var fs = File.OpenWrite(fileName))
                await SaveToStreamAsync(fs).ConfigureAwait(false);
        }
#endif

        public abstract void SaveToStream(Stream stream);
#if FEATURE_TAP
        public abstract Task SaveToStreamAsync(Stream stream);
#endif
    }
}
