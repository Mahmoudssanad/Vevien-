using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog_Website.Controllers
{
    public class SearchController(ISearchService _searchService, UserManager<ApplicationUser> _userManager) : Controller
    {
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Content("");

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null)
                return NotFound();

            var users = await _searchService.UsersSearch(currentUser.Id);

            users = users
                .Where(x => x.UserName != null && x.UserName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return PartialView("_UsersListPartial", users);
        }

    }
}
