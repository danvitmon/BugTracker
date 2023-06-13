namespace BugTracker.Services.Interfaces
{
    public interface IBTImageService
    {
        public Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file);

        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, int defaultImage);
    }
}
