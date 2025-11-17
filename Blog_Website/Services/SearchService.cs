using Blog_Website.Models.Data;
using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Blog_Website.Services
{
    public class SearchService : ISearchService
    {
        private readonly AppDbContext _context;
        private readonly IHttpContextAccessor _httpcontext;

        public SearchService(AppDbContext context, IHttpContextAccessor httpcontext)
        {
            _context = context;
            _httpcontext = httpcontext;
        }
        public async Task<List<ApplicationUser>> UsersSearch()
        {
            var currentUserId = _httpcontext.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

            var users = await _context.Users.Where(x => x.Id != currentUserId && !x.IsDeleted).ToListAsync();

            return users;
        }
    }
}
