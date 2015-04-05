using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace OpenDLC
{
    public partial class RsdfContainer : DownloadContainer<RsdfEntry>
    {
        public static async Task<RsdfContainer> FromFileAsync(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                throw new ArgumentNullException("fileName"); // TODO: nameof(fileName)

            using (var f = File.OpenRead(fileName))
                return await FromStreamAsync(f).ConfigureAwait(false);
        }
        public static async Task<RsdfContainer> FromStreamAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream"); // TODO: nameof(stream)

            var ms = stream as MemoryStream;
            if (ms != null)
            {
                var str = Encoding.UTF8.GetString(ms.ToArray());
                return FromString(str);
            }
            using (var reader = new StreamReader(stream))
            {
                var stringValue = await reader.ReadToEndAsync().ConfigureAwait(false);
                return FromString(stringValue);
            }
        }
        public override async Task SaveToStreamAsync(Stream stream)
        {
            if (stream == null)
                throw new ArgumentNullException("stream"); // TODO: nameof(stream)

            var ms = stream as MemoryStream;
            if (ms != null)
            {
                SaveToStream(ms);
                return;
            }
            var str = SaveAsString();
            Debug.Assert(!string.IsNullOrWhiteSpace(str));
            var stringBuffer = Encoding.UTF8.GetBytes(str);
            await stream.WriteAsync(stringBuffer, 0, stringBuffer.Length).ConfigureAwait(false);
        }
    }
}
