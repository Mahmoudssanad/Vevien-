using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;

namespace Blog_Website.ViewModel.Post
{
    public class PostViewModel
    {
        public int Id { get; set; }

        public IFormFile? ImageFile { get; set; }

        public string? Content { get; set; }

        public bool Visible { get; set; }

        public bool Public { get; set; }

        [HiddenInput]
        public string? ImageUrl { get; set; }
    }
}
