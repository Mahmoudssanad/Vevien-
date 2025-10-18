using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Post;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using System.Security.Claims;

namespace Blog_Website.Services
{
    public class PostService : IPostService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _http;
        private readonly IWebHostEnvironment _webHost;
        private readonly IFollowService _followService;

        public PostService(AppDbContext context, IHttpContextAccessor http, IWebHostEnvironment webHost, IFollowService followService)
        {
            _context = context;
            _http = http;
            _webHost = webHost;
            _followService = followService;
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
            }
            Console.WriteLine("Something invalid");
        }

        public async Task DeleteAsync(int postId)
        {
            var found = await _context.Posts.FirstOrDefaultAsync(x => x.Id == postId);

            if (found == null) ;

            _context.Posts.Remove(found);
            await _context.SaveChangesAsync();
        }

        public async Task<List<PostViewModel>> GetAllUserPostsAsync(string userId)
        {
            var userPosts = await _context.Posts
                .Include(x => x.ApplicationUser)
                .Where(x => x.UserId == userId && x.Visible)
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
                    .FirstOrDefaultAsync(x => x.Id == postId);

            return post;
        }

        public async Task<List<Post>> GetFriendsPosts()
        {
            var currentUserId = _http.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

            // Follow اللي انا عامل ليهم Users روحت عشان اجيب ال 
            var followedUsers = await _followService.GetFollowersAsync(currentUserId);

            var followedUsersId = followedUsers.Select(x => x.Id).ToList();

            var allFriendsPosts = await _context.Posts
                .Include(x => x.ApplicationUser)
                .Where(x => x.Visible && followedUsersId.Contains(x.UserId) || (x.UserId == currentUserId && x.Visible))
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return allFriendsPosts;
        }

        public async Task<List<Post>> GetPublicPosts()
        {
            var allpublicPosts = await _context.Posts
                .Include(x => x.ApplicationUser)
                .Where(x => x.Public && x.Visible)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

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
                    Public = x.Public
                })
                .ToListAsync();

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
    }
}
