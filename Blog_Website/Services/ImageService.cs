using Blog_Website.Services.IServices;

namespace Blog_Website.Services
{
    public class ImageService(IWebHostEnvironment _webHostEnvironment) : IImageService
    {
        public void DeleteImage(string imagePath)
        {
            if(string.IsNullOrEmpty(imagePath) || imagePath.Contains("default.png"))
                return;

            var relativePath = imagePath.TrimStart('/');
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        public async Task<string> UploadProfileImageAsync(IFormFile image)
        {
            var folder = Path.Combine(_webHostEnvironment.WebRootPath, "images/profile");
            Directory.CreateDirectory(folder); // if not found .. create

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

            var fullPath = Path.Combine(folder, fileName);

            using(Stream stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"/images/profile/{fileName}";
        }

        public async Task<string> UploadPostImageAsync(IFormFile image)
        {
            var folder = Path.Combine(_webHostEnvironment.WebRootPath, "images/post");
            Directory.CreateDirectory(folder); // if not found .. create

            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";

            var fullPath = Path.Combine(folder, fileName);

            using (Stream stream = new FileStream(fullPath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            return $"/images/post/{fileName}";
        }
    }
}
