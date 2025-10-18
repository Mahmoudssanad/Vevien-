using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Post;

namespace Blog_Website.Services.IServices
{
    public interface IPostService
    {
        Task AddAsync(PostViewModel model);
        Task UpdateAsync(PostViewModel newPost, int postId);
        Task DeleteAsync(int postId);
        Task<Post> GetByIdAsync(int postId);
        Task<List<PostViewModel>> GetAllUserPostsAsync(string userId);
        Task<List<PostViewModel>> MyPosts(string userId);
        Task<List<Post>> GetPublicPosts();
        Task<List<Post>> GetFriendsPosts();
    }
}
