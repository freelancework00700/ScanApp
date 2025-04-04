namespace ScanApp.Models
{
    public record FileHashes(
        string MD5,
        string SHA1,
        string SHA256
    );
}