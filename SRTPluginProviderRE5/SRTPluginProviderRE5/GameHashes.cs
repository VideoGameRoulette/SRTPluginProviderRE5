using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace SRTPluginProviderRE5
{
    /// <summary>
    /// SHA256 hashes for the RE5/BIO5 game executables.
    /// </summary>
    public static class GameHashes
    {
        private static readonly byte[] re5dx9WW_20200922_1 = new byte[32] { 0xF9, 0xD5, 0x04, 0x6D, 0x3C, 0x19, 0xC2, 0xDD, 0xE7, 0xB5, 0xAB, 0xC5, 0x11, 0x4A, 0x04, 0x2D, 0x6D, 0x36, 0xE7, 0x0E, 0x3F, 0xA2, 0x9D, 0x79, 0xDC, 0x53, 0x36, 0xD6, 0xE0, 0x3A, 0x0C, 0x1F }; // Steam WW RTM

        public static GameVersion DetectVersion(string filePath)
        {
            byte[] checksum;
            using (SHA256 hashFunc = SHA256.Create())
            using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                checksum = hashFunc.ComputeHash(fs);

            if (checksum.SequenceEqual(re5dx9WW_20200922_1))
            {
                Console.WriteLine("Steam v1.0 Detected.");
                return GameVersion.RE5DX9WW_20200922_1;
            }

            Console.WriteLine("Unknown Version. If you have installed any third party mods, please uninstall and try again.");
            return GameVersion.Unknown;
        }
    }
}