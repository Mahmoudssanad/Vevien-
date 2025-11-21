using Blog_Website.Generics;
using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Notification;
using Blog_Website.ViewModel.Posts;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace Blog_Website.Services
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly IFollowService _followService;
        private readonly INotificationService _notifiService;
        private readonly IImageService _imageService;
        private readonly ILogger<PostService> _logger;

        public PostService(AppDbContext context, IHttpContextAccessor http,
            IWebHostEnvironment webHost, IFollowService followService,
            INotificationService notifiService, IImageService imageService, ILogger<PostService> logger)
        {
            _context = context;
            _http = http;
            _followService = followService;
            _notifiService = notifiService;
            _imageService = imageService;
            _logger = logger;
        }

        public async Task AddAsync(PostViewModel model)
        {
            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var userId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new UnauthorizedAccessException("User is not authenticated");

            if (string.IsNullOrEmpty(model.Content) && model.ImageFile == null)
                throw new ValidationException("Post should contain either Content or Image");

            var user = await _context.Users.FindAsync(userId)
               ?? throw new InvalidOperationException("User not found");

            var newPost = new DesplayPostViewModel
            {
                UserId = userId,
                Visible = model.Visible,
                Content = model.Content,
                Public = model.Public,
                CreatedDate = DateTime.UtcNow,
            };

            // علشان ممكن ارفع الصوره ومعملش حفظ اصلا للبوست وبالتالي السيرفر هيخزن الصوره 
            using var transaction = await _context.Database.BeginTransactionAsync();

            // Add Post && upload image if found
            try
            {
                // Upload post image if found
                if (model.ImageFile != null)
                {
                    var result = await _imageService.UploadPostImageAsync(model.ImageFile);

                    newPost.ImageUrl = result;
                }

                // Add Post
                await _context.Posts.AddAsync(newPost);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            if (newPost.Visible || newPost.Public)
            {
                // Get followers => send notification for followers when Add post
                var followers = await _followService.GetFollowersAsync(userId);
                var redirectUrl = $"/Post/Details?postId={newPost.Id}";

                // use parallel task instead of for loop
                var tasks = followers.Where(x => !string.IsNullOrEmpty(x.Id))
                    .Select(follower => _notifiService.CreateAsync(new AddNotificationViewModel
                    {
                        SenderId = userId,
                        ReceiverId = follower.Id,
                        Type = "Post",
                        Title = $"{user.UserName} Add New Post",
                        Description = $"{user.UserName} Add New Post",
                        RedirectUrl = redirectUrl
                    }));
                await Task.WhenAll(tasks); // send only one request to database for all followers


                // Follower لكل DB Call يعني بيروح يعمل sequential الكود دا 
                //foreach (var follower in followers)
                //{
                //    var notification = new AddNotificationViewModel
                //    {
                //        SenderId = userId,
                //        ReceiverId = follower.Id,
                //        Type = "Post",
                //        Title = $"{user!.UserName} Add New Post",
                //        Description = $"{user!.UserName} Add New Post",
                //        RedirectUrl = redirectUrl
                //    };
                //    if (!string.IsNullOrEmpty(follower.Id))
                //        await _notifiService.CreateAsync(notification);
                //}
            }
        }   

        public async Task DeleteAsync(int postId)
        {
            var found = await _context.Posts
                .Include(x => x.Comments)
                .FirstOrDefaultAsync(x => x.Id == postId);
            if (found == null)
                throw new KeyNotFoundException($"Post with Id {postId} not found");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Delete all comments for this post(bulk delete)
                if(found.Comments.Any())
                    _context.Comments.RemoveRange(found.Comments);

                // Delete post
                _context.Posts.Remove(found!);
                await _context.SaveChangesAsync();

                // Delete image if found
                if (found.ImageUrl != null)
                    _imageService.DeleteImage(found.ImageUrl);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, $"Error Deleting post {found.Id}");
                throw;
            }
            
        }

        public async Task UpdateAsync(PostViewModel newPost, int postId)
        {
            var post = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);
            if (post == null)
                throw new Exception("Post not found.");

            var userId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (post.UserId != userId)
                throw new UnauthorizedAccessException("You are not allowed to edit this post.");

            if (newPost.Content is null && newPost.ImageFile is null)
                throw new ValidationException("Post must have content or an image.");

            var oldImage = post.ImageUrl;

            post.Public = newPost.Public;
            post.Visible = newPost.Visible;
            post.Content = newPost.Content;
            

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (newPost.ImageFile != null)
                {
                    var result = await _imageService.UploadPostImageAsync(newPost.ImageFile);
                    post.ImageUrl = result;
                }

                _context.Posts.Update(post);
                await _context.SaveChangesAsync();

                if (newPost.ImageFile != null && !string.IsNullOrEmpty(oldImage))
                    _imageService.DeleteImage(oldImage);

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured for updating {postId}");
                await transaction.RollbackAsync();
                throw;
            }
        }

        // with pagenation
        public async Task<PageResult<PostViewModel>> GetAllUserPostsAsync(string userId, string currentUserId, int pageSize, int pageNumber)
        {
            var query = _context.Posts
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.ApplicationUser.IsDeleted);
            if (userId != currentUserId)
            {
                query = query.Where(x => x.Visible);
            }

            var totalCount = await query.CountAsync();

            var items = await query.OrderByDescending(x => x.CreatedDate)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(x => new PostViewModel
                {
                    Id = x.Id,
                    Visible = x.Visible,
                    Content = x.Content,
                    ImageUrl = x.ImageUrl,
                    Public = x.Public,

                    // One query replaced foreach
                    IsLikedByCurrentUser = _context.Likes
                                .Any(l => l.UserId == currentUserId && l.TargetId == x.Id),

                    TempLikesCount = _context.Likes
                                .Count(l => l.TargetId == x.Id && !l.ApplicationUser!.IsDeleted)
                }).ToListAsync();

            return new PageResult<PostViewModel>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                UserId = userId
            };
        }

        // without pagenation
        public async Task<List<PostViewModel>> GallaryPosts(string userId, string currentUserId)
        {
            var posts = _context.Posts
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.ApplicationUser.IsDeleted);

            if (userId != currentUserId)
                posts = posts.Where(x => x.Visible);

            var items = await posts
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new PostViewModel
                {
                    Id = x.Id,
                    Content = x.Content,
                    ImageUrl = x.ImageUrl,
                    Public = x.Public,
                    Visible = x.Visible,
                    IsLikedByCurrentUser = _context.Likes.Where(l => l.UserId == currentUserId && l.TargetId == x.Id).Any(),
                    TempLikesCount = _context.Likes.Where(n => !n.ApplicationUser!.IsDeleted && n.TargetId == x.Id).Count()
                }).ToListAsync();

            return items;
        }

        public async Task<PostDetailsViewModel> GetByIdAsync(int postId, string currentUserId)
        {
            var post = await _context.Posts
                .AsNoTracking()
                .Where(x => x.Id == postId && !x.ApplicationUser.IsDeleted)
                .Select(x => new PostDetailsViewModel
                {
                    PostId = x.Id,
                    UserId = x.UserId,
                    Content = x.Content,
                    ImageUrl = x.ImageUrl,
                    CreatedDate = x.CreatedDate,
                    IsLikedByCurrentUser = _context.Likes.Where(l => l.UserId == currentUserId && l.TargetId == x.Id).Any(),
                    TempLikesCount = _context.Likes.Where(n => !n.ApplicationUser!.IsDeleted && n.TargetId == x.Id).Count(),
                    User = x.ApplicationUser,
                    // Include Comment
                    Comments = _context.Comments.Where(x => !x.ApplicationUser!.IsDeleted && x.PostId == postId)
                        .Select(c => new Comment
                        {
                            Id = c.Id,
                            Content = c.Content,
                            ApplicationUser = c.ApplicationUser,
                            ImageUrl = c.ImageUrl,
                            CreatedDate = c.CreatedDate,
                        }).ToList()
                }).FirstOrDefaultAsync();

            // Causes N+1 Proplem
            //post.TempLikesCount = await _context.Likes.CountAsync(x => x.TargetId == post.Id && !x.ApplicationUser!.IsDeleted);
            //post.IsLikedByCurrentUser = await _context.Likes.AnyAsync(x => x.TargetId == post.Id && x.UserId == userId);

            return post!;
        }

        public async Task<List<DisplayPostViewModel>> GetFriendsPosts(string currentUserId)
        {
            // Follow اللي انا عامل ليهم Users روحت عشان اجيب ال 
            var followedUsers = await _followService
                .GetFollowingsAsync(currentUserId);

            var followedUsersId = followedUsers.Select(x => x.Id).ToList();

            var query = _context.Posts
                .AsNoTracking()
                .Where(post =>
                    !post.ApplicationUser.IsDeleted &&
                    (
                        post.UserId == currentUserId                 // user’s own posts
                        ||
                        (post.Public && post.Visible)                // public posts
                        ||
                        (!post.Public && post.Visible && followedUsersId.Contains(post.UserId)) // private but from followed users
                    )
                );

            
            var posts = await query
                .OrderByDescending(x => x.CreatedDate)
                .Select(x => new DisplayPostViewModel
                {
                    PostId = x.Id,
                    ImageUrl = x.ImageUrl,
                    Content = x.Content,
                    CreatedDate = x.CreatedDate,
                    UserId = x.UserId,
                    User = x.ApplicationUser,
                    IsLikedByCurrentUser = _context.Likes.Where(x => x.UserId == currentUserId && x.TargetId == x.Id).Any(),
                    TempLikesCount = _context.Likes.Where(n => !n.ApplicationUser!.IsDeleted && n.TargetId == x.Id).Count(),
                })
                .ToListAsync();

            //Causes N+1 Problem
            //foreach(var post in allFriendsPosts)
            //{
            //    post.TempLikesCount = await _context.Likes.CountAsync(x => x.TargetId == post.Id && !x.ApplicationUser!.IsDeleted);
            //    post.IsLikedByCurrentUser = await _context.Likes
            //        .AnyAsync(x => x.UserId == _http.HttpContext!.User.FindFirstValue(ClaimTypes.NameIdentifier) && x.TargetId == post.Id);

            //}

            return posts;
        }

        public async Task<int> PostsCount(string userId, string currentUserId)
        {
            int visiblePostsCount;
            if (userId != currentUserId)
            {
                visiblePostsCount = await _context.Posts.CountAsync(x => x.UserId == userId && x.Visible);
            }
            else
            {
                visiblePostsCount = await _context.Posts.CountAsync(x => x.UserId == userId);
            }

            return visiblePostsCount;
        }

        public async Task<int> MyPostsCount(string userId)
        {
            var visiblePostsCount = await _context.Posts.CountAsync(x => x.UserId == userId);

            return visiblePostsCount;
        }
    }
}
