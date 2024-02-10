namespace TheBugTracker.Services.Interfaces
{
    public interface IBTImageService
    {
        Task<byte[]> EncodeImageAsync(IFormFile file);
        Task<byte[]> EncodeImageAsync(string fileName);
        string DecodeImage(byte[] data, string type);
        string ContentType(IFormFile file);
        int Size(IFormFile file);
    }
}
