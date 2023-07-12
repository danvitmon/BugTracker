using BugTracker.Models.Enums;

namespace BugTracker.Services.Interfaces;

public interface IBTFileService
{
  Task<byte[]>  ConvertFileToByteArrayAsync(IFormFile file);
  string?       ConvertByteArrayToFile     (byte[]?   fileData, string? extension, DefaultImage defaultImage);
  public string GetFileIcon                (string    file);
  public string FormatFileSize             (long      bytes);
}