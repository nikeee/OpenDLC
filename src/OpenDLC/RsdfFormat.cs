using System;
using System.Security.Cryptography;

namespace OpenDLC
{
    internal static class RsdfFormat
    {
        private static readonly byte[] Key = { 0x8C, 0x35, 0x19, 0x2D, 0x96, 0x4D, 0xC3, 0x18, 0x2C, 0x6F, 0x84, 0xF3, 0x25, 0x22, 0x39, 0xEB, 0x4A, 0x32, 0x0D, 0x25, 0x00, 0x00, 0x00, 0x00 };

        internal static readonly byte[] IV = { 0xA3, 0xD5, 0xA3, 0x3C, 0xB9, 0x5A, 0xC1, 0xF5, 0xCB, 0xDB, 0x1A, 0xD2, 0x5C, 0xB0, 0xA7, 0xAA };

        private static readonly Lazy<SymmetricAlgorithm> _algorithm = new(() =>
        {
            var res = Aes.Create();
            res.Padding = PaddingMode.None;
            res.FeedbackSize = 8;
            res.BlockSize = 128;
            res.KeySize = 128;
            res.Mode = CipherMode.CFB;
            res.Key = Key;
            return res;
        });

        internal static SymmetricAlgorithm Algorithm => _algorithm.Value;
    }
}
