using System;
using System.Text;

namespace OpenDLC
{
    internal static class DlcFormat
    {
        internal const string JdServiceUrl = "http://service.jdownloader.org/dlcrypt/service.php";
        internal const string RateLimitExceededKey = "2YVhzRFdjR2dDQy9JL25aVXFjQ1RPZ";
        internal const int TempKeyLength = 88;

        internal static string DecodeDataString(string encodedString)
        {
            return Encoding.UTF8.GetString(Convert.FromBase64String(encodedString));
        }
    }
}
