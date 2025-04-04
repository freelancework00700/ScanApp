using System.IO;
using System.Security.Cryptography;
using ScanApp.Models;
using ScanApp.Services.Interfaces;

namespace ScanApp.Services
{
    public class HashProvider : IHashProvider
    {
        public FileHashes ComputeHashes(string filePath)
        {
            using var md5 = MD5.Create();
            using var sha1 = SHA1.Create();
            using var sha256 = SHA256.Create();
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 8192);

            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
            {
                md5.TransformBlock(buffer, 0, bytesRead, null, 0);
                sha1.TransformBlock(buffer, 0, bytesRead, null, 0);
                sha256.TransformBlock(buffer, 0, bytesRead, null, 0);
            }

            md5.TransformFinalBlock(buffer, 0, 0);
            sha1.TransformFinalBlock(buffer, 0, 0);
            sha256.TransformFinalBlock(buffer, 0, 0);

            return new FileHashes(
                BitConverter.ToString(md5.Hash!).Replace("-", ""),
                BitConverter.ToString(sha1.Hash!).Replace("-", ""),
                BitConverter.ToString(sha256.Hash!).Replace("-", "")
            );
        }
    }
}