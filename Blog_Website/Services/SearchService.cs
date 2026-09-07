using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace Blog_Website.Services
{
    public class SearchService : ISearchService
    {
        private readonly AppDbContext _context;

        public SearchService(AppDbContext context)
        {
            _context = context;
        }
        public async Task<List<ApplicationUser>> UsersSearch(string currentUserId)
        {
            var users = await _context.Users.Where(x => x.Id != currentUserId && !x.IsDeleted).ToListAsync();

            return users;
        }
    }
}
