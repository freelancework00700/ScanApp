using ScanApp.Models;

namespace ScanApp.Services.Interfaces
{
    public interface IFileScanner
    {
        void ScanDirectory(string path);
        ScanResult Result { get; }
    }
}