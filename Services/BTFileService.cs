using BugTracker.Models.Enums;
using BugTracker.Services.Interfaces;

namespace BugTracker.Services;

public class BTFileService : IBTFileService
{
  private readonly string _defaultBtUserImageSrc  = String.Empty;
  private readonly string _defaultCompanyImageSrc = String.Empty;
  private readonly string _defaultImage           = String.Empty;
  private readonly string _defaultProjectImageSrc = String.Empty;

  private readonly string[] _suffixes = { "Bytes", "KB", "MB", "GB", "TB", "PB" };

  public string ConvertByteArrayToFile(byte[]? fileData, string? extension, DefaultImage defaultImage)
  {
    if (fileData == null)
      return defaultImage switch 
                          {
                            DefaultImage.BTUserImage  => _defaultBtUserImageSrc,
                            DefaultImage.CompanyImage => _defaultCompanyImageSrc,
                            DefaultImage.ProjectImage => _defaultProjectImageSrc, 
                            _                         => _defaultImage
                          };
    
    return string.Format($"data:{extension};base64,{Convert.ToBase64String(fileData)}");
  }

  public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile file)
  {
    using var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);
    
    return memoryStream.ToArray();
  }

  public string GetFileIcon(string file)
  {
    var fileExtension = Path.GetExtension(file).Replace(".", "");
    
    return $"/img/contenttype/{fileExtension}.png";
  }
  
  public string FormatFileSize(long bytes)
  {
    var     counter = 0;
    decimal number  = bytes;

    while (Math.Round(number / 1024) >= 1)
    {
      number /= 1024;
      counter++;
    }

    return $"{number:n1}{_suffixes[counter]}";
  }
}