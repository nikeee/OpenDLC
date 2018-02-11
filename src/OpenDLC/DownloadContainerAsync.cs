using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;

namespace OpenDLC
{
    public abstract partial class DownloadContainer<T> : Collection<T>
    {
        public async Task SaveToFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            using (var fs = File.OpenWrite(fileName))
                await SaveToStreamAsync(fs).ConfigureAwait(false);
        }

        public abstract Task SaveToStreamAsync(Stream stream);
    }
}
