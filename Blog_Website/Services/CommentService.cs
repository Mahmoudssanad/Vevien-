using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Comment;
using Microsoft.EntityFrameworkCore;

namespace Blog_Website.Services
{
    public class CommentService : ICommentService
    {
        private readonly AppDbContext _context;

        public CommentService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Comment> AddCommentAsync(CommentViewModel model)
        {
            var newComment = new Comment
            {
                UserId = model.UserId,
                Content = model.Content,
                CreatedDate = DateTime.Now,
                PostId = model.PostId,
            };

            await _context.Comments.AddAsync(newComment);
            await _context.SaveChangesAsync();

            var commentWithUser = await _context.Comments
                .Include(x => x.ApplicationUser)
                .FirstOrDefaultAsync(x => x.Id == newComment.Id);

            var viewModel = new CommentViewModel
            {
                UserId = commentWithUser.UserId,
                Content = commentWithUser.Content,
                PostId = commentWithUser.PostId,
                ImageUrl = commentWithUser.ApplicationUser?.ImageURL,
                // أضف خاصية UserName في CommentViewModel
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
                .Include(x => x.ApplicationUser)
                .Where(x => x.PostId == postId)
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
