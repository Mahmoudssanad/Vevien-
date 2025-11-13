using Blog_Website.Models.Entities;

namespace Blog_Website.Services.IServices
{
    public interface ISearchService
    {
        Task<List<ApplicationUser>> UsersSearch();
    }
}
