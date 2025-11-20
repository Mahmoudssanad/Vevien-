using Blog_Website.Models.Entities;
using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Post;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Blog_Website.Controllers
{
    //[Authorize]
    public class PostController : Controller
    {
        private readonly IPostService _postService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PostController(IPostService postService, UserManager<ApplicationUser> userManager)
        {
            _postService = postService;
            _userManager = userManager;
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
                catch
                {
                    ModelState.AddModelError("", "Can not share empty post");
                }
            }

            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                ModelState.AddModelError("", error.ErrorMessage);
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

        [HttpGet]
        public async Task<IActionResult> MyPosts(string userId)
        {
            var currentUser = await _userManager.GetUserAsync(User);

            if(currentUser == null) return Unauthorized();

            List<PostViewModel> myPosts;

            if (userId == currentUser.Id)
                myPosts = await _postService.MyPosts(userId);
            else
                myPosts = await _postService.GetAllUserPostsAsync(userId);

                return View(myPosts);
        }

    }
}
