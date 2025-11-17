namespace Blog_Website.Services.IServices
{
    public interface IImageService
    {
        Task<string> UploadProfileImageAsync(IFormFile image);

        void DeleteImage(string imagePath);
    }
}
