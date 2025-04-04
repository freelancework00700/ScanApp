namespace ScanApp.Models
{
    public record FileRecord(
        int Id,
        string Path,
        long Size,
        long LastModified,
        string Md5Hash,
        string Sha1Hash,
        string Sha256Hash,
        long LastSeen,
        int ScannedCount
    )
    {
        // Add parameterless constructor for Dapper
        public FileRecord() : this(
            default,
            default!,
            default,
            default,
            default!,
            default!,
            default!,
            default,
            default)
        {
        }
    };
}