using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenDLC
{
    public class DlcEntry : DownloadEntry
    {
        public DlcEntry(string url)
            : base(url)
        { }

        public static implicit operator string (DlcEntry value)
        {
            return value == null ? null : value.Url;
        }
    }
}
