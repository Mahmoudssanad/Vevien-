using Blog_Website.Generics;
using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Posts;

namespace Blog_Website.Services.IServices
{
    public interface IPostService
    {
        Task AddAsync(PostViewModel model);
        Task UpdateAsync(PostViewModel newPost, int postId);
        Task DeleteAsync(int postId);
        Task<PostDetailsViewModel> GetByIdAsync(int postId, string currentUserId);
        Task<PageResult<PostViewModel>> GetAllUserPostsAsync(string userId, string currentUserId, int pageSize, int pageNumber);
        Task<List<PostViewModel>> GallaryPosts(string userId, string currentUserId);
        //Task<List<DisplayPostViewModel>> GetPublicPosts();
        Task<List<DisplayPostViewModel>> GetFriendsPosts(string currentUserId);

        Task<int> PostsCount(string userId, string currentUserId);
        Task<int> MyPostsCount(string userId);

    }
}
