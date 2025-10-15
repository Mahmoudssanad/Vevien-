using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Post;
using Microsoft.AspNetCore.Http.HttpResults;
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

        public PostService(AppDbContext context, IHttpContextAccessor http, IWebHostEnvironment webHost)
        {
            _context = context;
            _http = http;
            _webHost = webHost;
        }

        public async Task AddAsync(PostViewModel model)
        {
            var userId = _http.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (model != null && userId != null)
            {
                string postPath = string.Empty;

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

        public async Task<List<Post>> GetAllUserPostsAsync(string userId)
        {
            var userPosts = await _context.Posts
                .Include(x => x.ApplicationUser)
                .Where(x => x.UserId == userId && x.Visible)
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
            // Add Follow in this service when finished
            var allFriendsPosts = await _context.Posts
                .Include(x => x.ApplicationUser)
                .Where(x => x.Visible)
                .ToListAsync();

            return allFriendsPosts;
        }

        public async Task<List<Post>> GetPublicPosts()
        {
            var allpublicPosts = await _context.Posts
                .Include(x => x.ApplicationUser)
                .Where(x => x.Public && x.Visible)
                .ToListAsync();

            return allpublicPosts;
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
