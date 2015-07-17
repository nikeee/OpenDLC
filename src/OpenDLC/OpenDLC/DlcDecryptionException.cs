using System;

namespace OpenDLC
{
    public class DlcDecryptionException : Exception
    { }
    public class DlcLimitExceededException : DlcDecryptionException
    { }
}
