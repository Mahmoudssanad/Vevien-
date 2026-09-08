using Blog_Website.Models.Entities;
using Blog_Website.ViewModel.Comments;

namespace Blog_Website.Services.IServices
{
    public interface ICommentService
    {
        Task<Comment> AddCommentAsync(CommentViewModel model);

        Task<bool> DeleteCommentAsync(int commentId, string userId);

        Task<List<Comment>> GetPostCommentsAsync(int postId);

        Task<bool> UpdateAsync(int commentId, Comment comment);

        Task<Comment> GetByIdAsync(int commentId);
    }
}
