using Blog_Website.Services.IServices;
using Blog_Website.ViewModel.Post;
using Microsoft.AspNetCore.Mvc;

namespace Blog_Website.Controllers
{
    public class PostController : Controller
    {
        private readonly IPostService _postService;

        public PostController(IPostService postService)
        {
            _postService = postService;
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
            Console.WriteLine("Add Action Triggered!");

            if (ModelState.IsValid)
            {
                await _postService.AddAsync(model);
                Console.WriteLine("Post added successfully!");

                return RedirectToAction("Index", "Home");
            }

            Console.WriteLine("ModelState is not valid!");
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

        public async Task<IActionResult> Delete(int postId)
        {
            await _postService.DeleteAsync(postId);

            return RedirectToAction("Index", "Home");
        }
    }
}
