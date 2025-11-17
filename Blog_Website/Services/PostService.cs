using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Notification;
using Blog_Website.ViewModel.Post;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Blog_Website.Services
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _webHost;
        private readonly IFollowService _followService;
        private readonly INotificationService _notifiService;

        public PostService(AppDbContext context, IHttpContextAccessor http,
            IWebHostEnvironment webHost, IFollowService followService, INotificationService notifiService)
        {
            _context = context;
            _http = http;
            _webHost = webHost;
            _followService = followService;
            _notifiService = notifiService;
        }

        public async Task AddAsync(PostViewModel model)
        {
            var userId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (model != null && userId != null)
            {
                string postPath = string.Empty;

                if (model.Content == null && model.ImageFile == null)
                    throw new Exception("Post should contains Image or Content");
                   

                if (model.ImageFile != null)
                {
                    var uploadFolder = Path.Combine(_webHost.WebRootPath, "images/posts");
                    Directory.CreateDirectory(uploadFolder);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(model.ImageFile.FileName)}";

                    var fullPath = Path.Combine(uploadFolder, fileName);

                    using (Stream stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await model.ImageFile.CopyToAsync(stream);
                    };

                    postPath = $"/images/posts/{fileName}";
                }

                var newPost = new Post
                {
                    UserId = userId,
                    Visible = model.Visible,
                    Content = model.Content,
                    Public = model.Public,
                    CreatedDate = DateTime.UtcNow,
                    ImageUrl = postPath,
                };

                await _context.Posts.AddAsync(newPost);
                await _context.SaveChangesAsync();

                
                if (newPost.Visible || newPost.Public)
                {
                    var followers = await _followService.GetFollowingsAsync(userId);
                    var user = await _context.Users.FindAsync(userId);

                    var redirectUrl = $"/Post/Details?postId={newPost.Id}";

                    foreach (var follower in followers)
                    {
                        var notification = new AddNotificationViewModel
                        {
                            SenderId = userId,
                            ReceiverId = follower.Id,
                            Type = "Post",
                            Title = $"{user!.UserName} Add New Post",
                            Description = $"{user!.UserName} Add New Post",
                            RedirectUrl = redirectUrl
                        };
                        if (!string.IsNullOrEmpty(follower.Id))
                            await _notifiService.CreateAsync(notification);
                    }
                }
            }
            
        }

        public async Task DeleteAsync(int postId)
        {
            var found = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);

            _context.Posts.Remove(found!);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PostViewModel>> GetAllUserPostsAsync(string userId)
        {
            var userPosts = await _context.Posts
                .Include(x => x.ApplicationUser)
                .Where(x => x.UserId == userId && x.Visible && !x.ApplicationUser!.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new PostViewModel
                {
                    Id = x.Id,
                    Visible = x.Visible,
                    Content = x.Content,
                    ImageUrl = x.ImageUrl,
                    Public = x.Public
                })
                .ToListAsync();

            return userPosts;
        }

        public async Task<Post> GetByIdAsync(int postId)
        {
            var post = await _context.Posts
                .Include(x => x.ApplicationUser)
                .Include(x => x.Comments.Where(x => !x.ApplicationUser!.IsDeleted))
                    .ThenInclude(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == postId && !x.ApplicationUser.IsDeleted);

            if (post == null)
                return null!;

            var userId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            post.TempLikesCount = await _context.Likes.CountAsync(x => x.TargetId == post.Id && !x.ApplicationUser!.IsDeleted);
            post.IsLikedByCurrentUser = await _context.Likes.AnyAsync(x => x.TargetId == post.Id && x.UserId == userId);

            return post;
        }

        public async Task<List<Post>> GetFriendsPosts()
        {
            var currentUserId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Follow اللي انا عامل ليهم Users روحت عشان اجيب ال 
            var followedUsers = await _followService.GetFollowingsAsync(currentUserId!);

            var followedUsersId = followedUsers.Select(x => x.Id).ToList();

            var allFriendsPosts = await _context.Posts
                .Include(x => x.ApplicationUser)
                .Where(x => x.Visible && followedUsersId.Contains(x.UserId)
                && !x.ApplicationUser.IsDeleted
                || (x.UserId == currentUserId && x.Visible))
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            // Include Likes Implicitly
            foreach(var post in allFriendsPosts)
            {
                post.TempLikesCount = await _context.Likes.CountAsync(x => x.TargetId == post.Id && !x.ApplicationUser!.IsDeleted);
                post.IsLikedByCurrentUser = await _context.Likes
                    .AnyAsync(x => x.UserId == _http.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier) && x.TargetId == post.Id);

            }

            return allFriendsPosts;
        }

        public async Task<List<Post>> GetPublicPosts()
        {
            var allpublicPosts = await _context.Posts
                .Include(x => x.ApplicationUser)
                .Where(x => x.Public && x.Visible && !x.ApplicationUser.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            // Include Likes Implicitly
            foreach (var post in allpublicPosts)
            {
                post.TempLikesCount = await _context.Likes.CountAsync(l => l.TargetId == post.Id && !l.ApplicationUser!.IsDeleted);
                post.IsLikedByCurrentUser = await _context.Likes
                    .AnyAsync(x => x.UserId == _http.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier) && x.TargetId == post.Id);
            }

            return allpublicPosts;

        }

        public async Task<List<PostViewModel>> MyPosts(string userId)
        {
            var myPosts = await _context.Posts
                .Where(x => x.UserId == userId)
                .Select(x => new PostViewModel
                {
                    Id = x.Id,
                    Visible = x.Visible,
                    Content = x.Content,
                    ImageUrl = x.ImageUrl,
                    Public = x.Public,
                    UserId = x.UserId
                })
                .ToListAsync();

            foreach (var post in myPosts)
            {
                post.IsLikedByCurrentUser = await _context.Likes
                    .AnyAsync(x => x.UserId == _http.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier) && x.TargetId == post.Id);
                post.TempLikesCount = await _context.Likes.CountAsync(x => x.TargetId ==  post.Id && !x.ApplicationUser!.IsDeleted);
            }

            return myPosts;
        }

        public async Task UpdateAsync(PostViewModel newPost, int postId)
        {
            var post = await _context.Posts
                .FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
                throw new Exception("Post not found.");

            var userId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this post.");

            if(newPost.ImageFile != null)
            {
                var uploadFolder = Path.Combine(_webHost.WebRootPath, "images/posts");
                Directory.CreateDirectory(uploadFolder);

                var uniqueImagePath = $"{Guid.NewGuid()}{Path.GetExtension(newPost.ImageFile.FileName)}";

                var fullPath = Path.Combine(uploadFolder, uniqueImagePath);

                using (Stream stream = new FileStream(fullPath, FileMode.Create))
                {
                    await newPost.ImageFile.CopyToAsync(stream);
                };
                post.ImageUrl = $"/images/posts/{uniqueImagePath}";
            }

            post.Public = newPost.Public;
            post.Visible = newPost.Visible;
            post.Content = newPost.Content;
            post.Id = postId;

            post.UserId = userId;

            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
        }

        public async Task<int> VisiblePostsCount(string userId)
        {
            var visiblePostsCount = await _context.Posts.CountAsync(x => x.UserId == userId && x.Visible);

            return visiblePostsCount;
        }

        public async Task<int> MyPostsCount(string userId)
        {
            var visiblePostsCount = await _context.Posts.CountAsync(x => x.UserId == userId);

            return visiblePostsCount;
        }
    }
}
