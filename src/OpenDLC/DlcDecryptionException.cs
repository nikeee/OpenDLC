using System;

namespace OpenDLC
{
    public class DlcDecryptionException(string message) : Exception(message) { }

    public class DlcLimitExceededException : DlcDecryptionException
    {
        public DlcLimitExceededException()
            : base("DLC decryption limit exceeded. Try again later.")
        { }
    }
}
