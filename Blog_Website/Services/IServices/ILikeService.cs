using Blog_Website.Enums;
using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Like;

namespace Blog_Website.Services.IServices
{
    public interface ILikeService
    {
        Task<bool> LikeAsync(int targetId, string userId, LikeTargetType targetType);

        Task<bool> UnlikeAsync(int targetId, string userId, LikeTargetType targetType);

        Task<bool> IsLikedAsync(int targetId, string userId, LikeTargetType targetType);

        Task<int> LikesCountAsync(int targetId, LikeTargetType targetType);

        Task<List<ApplicationUser>> GetAllLikesAsync(int targetId, LikeTargetType targetType);
    }
}
