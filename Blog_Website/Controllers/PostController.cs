using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Post;
using Blog_Website.ViewModel.Profile;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Blog_Website.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IFollowService _followService;

        public PostController(IPostService postService, UserManager<ApplicationUser> userManager, IFollowService followService)
        {
            _postService = postService;
            _userManager = userManager;
            _followService = followService;
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(PostViewModel model)
        {

            if (ModelState.IsValid)
            {
                try
                {
                    await _postService.AddAsync(model);

                    return RedirectToAction("Index", "Home");
                }
                catch(Exception ex)
                {
                    ModelState.AddModelError("", "Can not share empty post");
                }
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int postId)
        {
            var post = await _postService.GetByIdAsync(postId);

            var currentPost = new PostViewModel
            {
                Content = post.Content,
                Public = post.Public,
                Visible = post.Visible,
                ImageUrl = post.ImageUrl,
                Id = post.Id
            };

            return View(currentPost);
        }

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public async Task<IActionResult> Edit(PostViewModel model, int postId)
        {
            if (ModelState.IsValid)
            {
                await _postService.UpdateAsync(model, postId);

                return RedirectToAction("Index", "Home");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int postId)
        {
            await _postService.DeleteAsync(postId);

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> Details(int postId)
        {
            var post = await _postService.GetByIdAsync(postId);

            if (post == null) return NotFound();

            return View(post);
        }

        

    }
}
