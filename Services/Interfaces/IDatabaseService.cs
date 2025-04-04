using ScanApp.Models;
namespace ScanApp.Services.Interfaces
{
    public interface IDatabaseService
    {
        void Initialize();
        bool TryGetExistingRecord(string path, FileInfo fileInfo, out FileRecord record);
        void UpdateLastSeen(FileRecord record);
        void StoreFileRecord(string path, FileInfo fileInfo, FileHashes hashes);
    }
}