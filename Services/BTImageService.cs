using BugTracker.Services.Interfaces;

namespace BugTracker.Services
{
    public class BTImageService : IBTImageService
    {

        private readonly string? _defaultUserImage = "/img/DefaultContactImage.png";
        private readonly string? _defaultCategoryImage = "/img/DefaultCategoryImage.png";
        private readonly string? _defaultBlogImage = "/img/DefaultBlogImage.png";

        public string? ConvertByteArrayToFile(byte[]? fileData, string? extension, int defaultImage)
        {
            if (fileData == null || fileData.Length == 0)
            {
                switch (defaultImage)
                {
                    //Return the default user image if the value is 1
                    case 1: return _defaultUserImage;
                    //Return the default blog image if the value is 2
                    case 2: return _defaultBlogImage;
                    //Return the default category image if the value is 3
                    case 3: return _defaultCategoryImage;
                }
            }

            try
            {
                string? imageBase64Data = Convert.ToBase64String(fileData);
                imageBase64Data = string.Format($"data:{extension};base64,{imageBase64Data}");

                return imageBase64Data;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<byte[]> ConvertFileToByteArrayAsync(IFormFile? file)
        {
            try
            {
                using MemoryStream memoryStream = new MemoryStream();
                await file!.CopyToAsync(memoryStream);
                byte[] byteFile = memoryStream.ToArray();
                memoryStream.Close();

                return byteFile;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
