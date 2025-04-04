// Services/Interfaces/IFileSystemInspector.cs
using System.Collections.Generic;

namespace ScanApp.Services.Interfaces
{
    public interface IFileSystemInspector
    {
        IEnumerable<string> SafeEnumerateFiles(string path);
        FileInfo GetFileInfo(string path);
        bool IsAccessible(string path);
    }
}