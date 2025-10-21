using System.Diagnostics;
using Blog_Website.Models;
using Blog_Website.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Blog_Website.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IPostService _postService;

        public HomeController(ILogger<HomeController> logger, IPostService postService)
        {
            _logger = logger;
            _postService = postService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var allPublicPosts = await _postService.GetPublicPosts();

            return View(allPublicPosts);
        }

        [HttpGet]
        public async Task<IActionResult> FriendsPosts()
        {
            var friendsPosts = await _postService.GetFriendsPosts();

            return View(friendsPosts);
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
