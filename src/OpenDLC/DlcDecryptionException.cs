using System;

namespace OpenDLC
{
    public class DlcDecryptionException : Exception
    {
        public DlcDecryptionException(string message)
            : base(message)
        { }
    }

    public class DlcLimitExceededException : DlcDecryptionException
    {
        public DlcLimitExceededException()
            : base("DLC decryption limit exceeded. Try again later.")
        { }
    }
}
