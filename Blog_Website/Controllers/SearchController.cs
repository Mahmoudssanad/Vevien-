using Blog_Website.Services.IServices;
using Microsoft.AspNetCore.Mvc;

namespace Blog_Website.Controllers
{
    public class SearchController(ISearchService _searchService) : Controller
    {
        public async Task<IActionResult> SearchUsers(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Content("");

            var users = await _searchService.UsersSearch();

            users = users
                .Where(x => x.UserName != null && x.UserName.Contains(query, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return PartialView("_UsersListPartial", users);
        }

    }
}
