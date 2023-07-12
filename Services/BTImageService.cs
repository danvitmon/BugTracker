using BugTracker.Services.Interfaces;

namespace BugTracker.Services;

public class BTImageService : IBTImageService
{
  private readonly string? _defaultBlogImage     = "/img/DefaultBlogImage.png";
  private readonly string? _defaultCategoryImage = "/img/DefaultCategoryImage.png";
  private readonly string? _defaultUserImage     = "/img/DefaultContactImage.png";

  public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, int defaultImage)
  {
    if (fileData == null || fileData.Length == 0)
      switch (defaultImage)
      {
        //Return the default user image if the value is 1
        case 1: return _defaultUserImage;
        //Return the default blog image if the value is 2
        case 2: return _defaultBlogImage;
        //Return the default category image if the value is 3
        case 3: return _defaultCategoryImage;
      }

    var imageBase64Data = Convert.ToBase64String(fileData);
    imageBase64Data = string.Format($"data:{extension};base64,{imageBase64Data}");

    return imageBase64Data;
  }

  public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
  {
    using var memoryStream = new MemoryStream();
    await file!.CopyToAsync(memoryStream);

    var byteFile = memoryStream.ToArray();

    memoryStream.Close();

    return byteFile;
  }
}