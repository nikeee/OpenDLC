using System;
using System.IO;
using System.Threading.Tasks;

namespace OpenDLC
{
    public partial class CcfContainer
    {
        public static async Task<CcfContainer> FromFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException(nameof(fileName));

            using var f = File.OpenRead(fileName);
            return await FromStreamAsync(f).ConfigureAwait(false);
        }

        public static async Task<CcfContainer> FromStreamAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            if (stream is MemoryStream alreadyMs)
                return FromBuffer(alreadyMs.ToArray());

            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms).ConfigureAwait(false);
            return FromBuffer(ms.ToArray());
        }

        public override async Task SaveToStreamAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException(nameof(stream));

            var buffer = SaveToBuffer();
            await stream.WriteAsync(buffer).ConfigureAwait(false);
        }
    }
}
