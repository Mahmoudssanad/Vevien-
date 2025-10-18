using Microsoft.AspNetCore.Mvc;
using NuGet.Protocol.Core.Types;
using System.ComponentModel.DataAnnotations;

namespace Blog_Website.ViewModel.Post
{
    public class PostViewModel
    {
        public int Id { get; set; }

        public IFormFile? ImageFile { get; set; }

        [MinLength(2, ErrorMessage = "Content should be greater than 2 and less than 800 charachtier")]
        [MaxLength(800, ErrorMessage = "Content should be greater than 2 and less than 800 charachtier")]
        public string? Content { get; set; }

        public bool Visible { get; set; }

        public bool Public { get; set; }

        [HiddenInput]
        public string? ImageUrl { get; set; }
    }
}
