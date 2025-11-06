using Blog_Website.Enums;
using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Notification;
using Microsoft.EntityFrameworkCore;

namespace Blog_Website.Services
{
    public class LikeService : ILikeService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public LikeService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<ApplicationUser>> GetAllLikesAsync(int targetId, LikeTargetType targetType)
        {
            var usersLike = await _context.Likes
                .Where(x => x.TargetId == targetId && x.TargetType == targetType)
                .Select(x => x.ApplicationUser).ToListAsync();

            return usersLike;
        }

        public async Task<bool> IsLikedAsync(int targetId, string userId, LikeTargetType targetType)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(x => x.TargetType == targetType && x.TargetId == targetId && x.UserId == userId);

            if(existingLike == null)
                return false;

            return true;
        }

        public async Task<bool> LikeAsync(int targetId, string userId, LikeTargetType targetType)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(x => x.TargetId == targetId && x.UserId == userId);

            if (existingLike != null)
                return false;

            var newLike = new Like
            {
                UserId = userId,
                TargetId = targetId,
                TargetType = targetType
            };

            await _context.Likes.AddAsync(newLike);
            await _context.SaveChangesAsync();

            string? receiverId = null;
            string redirectUrl = "";

            if (targetType == LikeTargetType.Like)
            {
                var post = await _context.Posts
                .Include(p => p.ApplicationUser) // علشان نجيب صاحب البوست
                .FirstOrDefaultAsync(p => p.Id == targetId);

                if (post != null)
                {
                    receiverId = post.UserId;
                    redirectUrl = $"/Post/Details?postId={post.Id}";
                }
                else
                {
                    return false;
                }
            }

            // ✅ الخطوة 2: نمنع إرسال إشعار لنفس الشخص اللي عمل لايك لمنشوره
            if (receiverId == userId || receiverId == null)
                return true;

            var notification = new AddNotificationViewModel
            {
                SenderId = userId,
                ReceiverId = receiverId,
                Title = "New Like",
                Description = $"{userId} add new like for your post",
                RedirectUrl = redirectUrl,
                Type = "Like",
                TargetId = targetId
            };

            await _notificationService.CreateAsync(notification);

            return true;
        }

        public async Task<int> LikesCountAsync(int targetId, LikeTargetType targetType)
        {
            var likeCounts = await _context.Likes
                .Where(x => x.TargetId == targetId && x.TargetType == targetType)
                .CountAsync();

            return likeCounts;
        }

        public async Task<bool> UnlikeAsync(int targetId, string userId, LikeTargetType targetType)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(x => x.TargetId == targetId && x.UserId == userId && x.TargetType == targetType);

            if (existingLike == null)
                return false;

            _context.Likes.Remove(existingLike);
            await _context.SaveChangesAsync();

            var notification = await _context.Notifications.FirstOrDefaultAsync(x => x.TargetId == targetId && x.SenderId == userId && x.Type == "Like");

            if (notification != null)
            {
                _context.Notifications.Remove(notification);
                await _context.SaveChangesAsync();
            }

            return true;
        }
    }
}
