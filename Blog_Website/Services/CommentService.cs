using Blog_Website.Hubs;
using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Comment;
using Blog_Website.ViewModel.Notification;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Blog_Website.Services
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly IHubContext<CommentHub> _hubContext;
        private readonly IViewRenderService _viewRenderService;

        public CommentService(AppDbContext context, INotificationService notificationService, IHubContext<CommentHub> hubContext, IViewRenderService viewRenderService)
        {
            _context = context;
            _notificationService = notificationService;
            _hubContext = hubContext;
            _viewRenderService = viewRenderService;
        }
        public async Task<Comment> AddCommentAsync(CommentViewModel model)
        {
            var newComment = new Comment
            {
                UserId = model.UserId,
                Content = model.Content,
                CreatedDate = DateTime.UtcNow,
                PostId = model.PostId,
            };

            await _context.Comments.AddAsync(newComment);
            await _context.SaveChangesAsync();

            var commentWithUser = await _context.Comments
                .Include(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == newComment.Id);


            #region Add notification when added comment of post
            //var userPost = await _context.Posts
            //    .Include(x => x.ApplicationUser)
            //    .FirstOrDefaultAsync(x => x.Id == model.PostId);

            //var userCommented = await _context.Users.FindAsync(model.UserId);

            //var notification = new AddNotificationViewModel
            //{
            //    SenderId = model.UserId,
            //    ReceiverId = userPost!.UserId,
            //    RedirectUrl = $"/Post/Details?postId={model.PostId}",
            //    Title = $"{userCommented!.UserName} Add comment for your post",
            //    Description = $"{userCommented!.UserName} Add comment for your post",
            //    Type = "Comment"
            //};

            //await _notificationService.CreateAsync(notification);
            #endregion

            var viewModel = new CommentViewModel
            {
                UserId = commentWithUser!.UserId!,
                Content = commentWithUser.Content,
                PostId = commentWithUser.PostId,
                ImageUrl = commentWithUser.ApplicationUser?.ImageURL,
                UserName = commentWithUser.ApplicationUser?.UserName,
                CreatedDate = commentWithUser.CreatedDate
            };

            return commentWithUser!;
        }

        public async Task<bool> DeleteCommentAsync(int commentId, string userId)
        {
            var existingComment = await _context.Comments
                .FirstOrDefaultAsync(x => x.Id == commentId && x.UserId == userId);

            if (existingComment != null)
            {
                _context.Comments.Remove(existingComment);
                await _context.SaveChangesAsync();

                return true;
            }
            return false;
        }

        public Task<Comment> GetByIdAsync(int commentId)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Comment>> GetPostCommentsAsync(int postId)
        {
            var postComments = await _context.Comments
                .AsNoTracking()
                .Include(x => x.ApplicationUser)
                .Where(x => x.PostId == postId 
                    && x.ApplicationUser != null
                    && !x.ApplicationUser!.IsDeleted)
                .OrderByDescending(x => x.CreatedDate)
                .ToListAsync();

            return postComments;
        }

        public Task<bool> UpdateAsync(int commentId, Comment comment)
        {
            throw new NotImplementedException();
        }
    }
}
