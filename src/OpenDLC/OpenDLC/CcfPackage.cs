using OpenDLC.Collections.Generic;
using System.Diagnostics;

namespace OpenDLC
{
    public class CcfPackage : DownloadPackage<CcfEntry>
    {
        public string Name { get; set; }

        // TODO: Put Comment & Password into "options" subtype?
        public string Comment { get; set; }
        public string Password { get; set; }
        public string Service { get; set; }
        public string Url { get; set; }

        public CcfPackage()
        {
            // Nothing?
        }

        internal CcfPackage(CcfPackageItem item)
        {
            Debug.Assert(item != null);
            Name = item.Name;
            Comment = item.Options.Kommentar;
            Password = item.Options.Passwort;
            Service = item.Service;
            Url = item.Url;

            var dls = item.Downloads;
            if (!CollectionEx.IsNullOrEmpty(dls))
            {
                for (int i = 0; i < dls.Count; ++i)
                {
                    var currentDownload = dls[i];
                    if (currentDownload != null)
                        Add(new CcfEntry(currentDownload));
                }
            }
        }
    }
}
