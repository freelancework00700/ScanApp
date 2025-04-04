using System.IO;

namespace ScanApp.Services.Interfaces
{
    public interface IPlatformService
    {
        string NormalizePath(string path);
        bool ShouldSkipPath(string path);
        FileAttributes GetExtendedAttributes(string path);
    }
}