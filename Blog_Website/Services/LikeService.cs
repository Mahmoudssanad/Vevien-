using Blog_Website.Enums;
using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Like;
using Microsoft.EntityFrameworkCore;

namespace Blog_Website.Services
{
    public class LikeService : ILikeService
    {
        private readonly AppDbContext _context;

        public LikeService(AppDbContext context)
        {
            _context = context;
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
            return true;
        }
    }
}
