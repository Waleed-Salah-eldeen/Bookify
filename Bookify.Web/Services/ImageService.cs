
using Bookify.Web.Core.Models;

namespace Bookify.Web.Services
{
    public class ImageService : IImageService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private List<string> _allowedExtentions = new() { ".jpg", ".jpeg", ".png" };
        private int _maxAllowedSize = 2097152; // 2 m-byte

        public ImageService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<(bool isUploded, string? erorrMassage)> UploadAsync(IFormFile image,
                     string imageName, string folderPath, bool hasThumbnail)
        {
            var extension = Path.GetExtension(image.FileName);

            if (!_allowedExtentions.Contains(extension))
                 return (false, ErrorMessages.NotAllowedExtention);

            if (image.Length > _maxAllowedSize)
                return (false, ErrorMessages.MaxSize);

            var path = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}", imageName);
            using var stream = File.Create(path);
            await image.CopyToAsync(stream);
            stream.Dispose();

            if(hasThumbnail)
            {
                var thumbPath = Path.Combine($"{_webHostEnvironment.WebRootPath}{folderPath}/thumb", imageName);
                using var loadedImage = Image.Load(image.OpenReadStream());
                var ratio = (float)loadedImage.Width / 200;
                var height = loadedImage.Height / ratio;
                loadedImage.Mutate(i => i.Resize(width: 200, height: (int)height));
                loadedImage.Save(thumbPath);
            }
            return (true,null);
        }

        public void Delete(string imagePath, string? imageThumbnailPath = null)
        {
            if (File.Exists($"{_webHostEnvironment.WebRootPath}{imagePath}"))
                File.Delete($"{_webHostEnvironment.WebRootPath}{imagePath}");

            if (!string.IsNullOrEmpty(imageThumbnailPath))
            {
                if (File.Exists($"{_webHostEnvironment.WebRootPath}{imageThumbnailPath}"))
                    File.Delete($"{_webHostEnvironment.WebRootPath}{imageThumbnailPath}");
            } 
        }

    }
}
