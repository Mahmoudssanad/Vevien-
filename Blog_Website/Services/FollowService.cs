using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Notification;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Blog_Website.Services
{
    public class FollowService : IFollowService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;

        public FollowService(AppDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }
        public async Task FollowAsync(string followerId, string followingId)
        {
            if (followerId == followingId) return;

            var exists = await _context.Follows
                .Include(x => x.Following)
                .Include(x => x.Follower)
                .AnyAsync(x => x.FollowerId == followerId && x.FollowingId == followingId);

            if (exists) return;

            var newFollow = new Follow
            {
                CreatedDate = DateTime.Now,
                FollowerId = followerId,
                FollowingId = followingId
            };
            try
            {
                await _context.Follows.AddAsync(newFollow);
                await _context.SaveChangesAsync();

                var redirectUrl = $"/Profile/Profile?userId={followingId}";

                var notification = new AddNotificationViewModel
                {
                    SenderId = followingId,
                    ReceiverId = followerId,
                    Description = $"Start following you",
                    Title = $"New Follow for you from...",
                    Type = "Follow",
                    RedirectUrl = redirectUrl
                };

                await _notificationService.CreateAsync(notification);
            }
            catch
            {
                throw new Exception("error when add follow in database");
            }
        }

        public async Task UnfollowAsync(string followerId, string followingId)
        {
            var foundFollowing = await _context.Follows.FirstOrDefaultAsync(x =>
                x.FollowingId == followingId && x.FollowerId == followerId);

            if (foundFollowing == null)
                throw new Exception("There is no following");

            _context.Follows.Remove(foundFollowing);
            await _context.SaveChangesAsync();
        }

        public async Task<int> FollowersCountAsync(string userId)
        {
            //var followersCount = await _context.Follows
            //    .Where(x => x.FollowingId == userId)
            //    .CountAsync();

            var followersCount = await _context.Follows.CountAsync(x => x.FollowingId == userId);

            return followersCount;
        }

        public async Task<int> FollowingCountAsync(string userId)
        {
            //var followingCount = await _context.Follows
            //    .Where(x => x.FollowerId == userId)
            //    .CountAsync();

            var followingCount = await _context.Follows.CountAsync(x => x.FollowerId == userId);

            return followingCount;
        }

        public async Task<List<ApplicationUser>> GetFollowersAsync(string userId)
        {
            var followers = await _context.Follows
                .Where(x => x.FollowingId == userId)
                .Select(x => x.Follower)
                .ToListAsync();

            return followers;
        }

        public async Task<List<ApplicationUser>> GetFollowingsAsync(string userId)
        {
            var following = await _context.Follows
                .Where(x => x.FollowerId == userId)
                .Select(x => x.Following)
                .ToListAsync();

            return following;
        }

        public async Task<bool> IsFollowingAsync(string followerId, string followedId)
        {
            var result = await _context.Follows.AnyAsync(x => x.FollowingId == followedId && x.FollowerId == followerId);

            return result;
        }
    }
}
