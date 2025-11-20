namespace Blog_Website.Services.IServices
{
    public interface IImageService
    {
        Task<string> UploadProfileImageAsync(IFormFile image);

        Task<string> UploadPostImageAsync(IFormFile image);

        void DeleteImage(string imagePath);
    }
}
