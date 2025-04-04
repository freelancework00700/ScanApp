using ScanApp.Models;

namespace ScanApp.Services.Interfaces
{
    public interface IHashProvider
    {
        FileHashes ComputeHashes(string filePath);
    }
}