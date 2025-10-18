using Blog_Website.Models.Entities;

namespace Blog_Website.Services.IServices
{
    public interface IFollowService
    {
        Task FollowAsync(string followerId, string followingId);

        Task UnfollowAsync(string followerId, string followingId);

        Task<List<ApplicationUser>> GetFollowersAsync(string userId);

        Task<List<ApplicationUser>> GetFollowingsAsync(string userId);

        Task<int> FollowersCountAsync(string userId);

        Task<int> FollowingCountAsync(string userId);

        Task<bool> IsFollowingAsync(string followerId, string followedId);
    }
}
