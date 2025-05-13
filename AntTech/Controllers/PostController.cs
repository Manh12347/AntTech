using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AntTech.Models;

namespace AntTech.Controllers
{
    public class PostController : Controller
    {
        private readonly BlogWebsiteDbContext _context;
        private static Article _draftArticle;
        private static List<PhotoInArticle> _photos = new List<PhotoInArticle>();
        private static List<TagInArticle> _tags = new List<TagInArticle>();
        private static List<Author> _authors = new List<Author>();
        private static List<Tag> tags;

        public PostController(BlogWebsiteDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            tags = _context.Tags.ToList();
        }

        public IActionResult Index(bool restore = false)
        {
            if (restore && _draftArticle != null)
            {
                var model = new EditorViewModel
                {
                    Title = _draftArticle.Title,
                    Content = _draftArticle.Content,
                    ImageUrls = _photos.Select(p => p.Photo).ToList()
                };
                return View(model);
            }
            return View(new EditorViewModel());
        }

        public IActionResult SavePost()
        {
            ViewBag.Tags = tags;
            return View(_draftArticle);
        }

        [HttpPost]
        public IActionResult SavePost([FromBody] SavePostDto model)
        {
            if (string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Content))
            {
                return BadRequest("Title and content are required.");
            }

            var article = new Article
            {
                Title = model.Title,
                Content = model.Content,
                StatusSet = "D",
                PublishDate = null,
                UpdateDate = DateTime.Now
            };

            _draftArticle = article;
            _photos.Clear();

            if (model.ImageUrls != null && model.ImageUrls.Any())
            {
                foreach (var imageUrl in model.ImageUrls)
                {
                    int position = model.Content.IndexOf(imageUrl);
                    _photos.Add(new PhotoInArticle
                    {
                        Photo = imageUrl,
                        positsion = position >= 0 ? position : 0
                    });
                }
            }

            return RedirectToAction("SavePost");
        }

        
        public class SavePostDto
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public List<string> ImageUrls { get; set; }
        }

        public class ConfirmSaveDto
        {
            public string Title { get; set; }
            public string Content { get; set; }
            public List<string> ImageUrls { get; set; }
            public List<int> CategoryIds { get; set; }
            public string CoverImageUrl { get; set; }
        }


        [HttpPost]
        [Route("/api/upload")]
        public async Task<IActionResult> UploadImage(IFormFile image)
        {
            if (image == null || image.Length == 0)
                return BadRequest("No image uploaded.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            var imageUrl = Url.Content("~/uploads/" + fileName);
            return Json(new { url = imageUrl });
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmSave([FromBody] ConfirmSaveDto model)
        {
            try
            {
                Console.WriteLine($"Received ConfirmSaveDto: Title={model.Title}, ImageUrlsCount={model.ImageUrls?.Count ?? 0}, CoverImageUrl={model.CoverImageUrl}");

                if (string.IsNullOrEmpty(model.Title) || string.IsNullOrEmpty(model.Content))
                {
                    return BadRequest("Title and content are required.");
                }

                var article = new Article
                {
                    Title = model.Title,
                    Content = model.Content, // Save with \n and ![Image](...)
                    StatusSet = "P",
                    PublishDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    LikeCount = 0,
                    DislikeCount = 0,
                    ViewCount = 0,
                    Photos = new List<PhotoInArticle>(),
                    ArticleTags = new List<TagInArticle>(),
                    Authors = new List<Author>()
                };

                // Add content image URLs
                if (model.ImageUrls != null && model.ImageUrls.Any())
                {
                    foreach (var imageUrl in model.ImageUrls)
                    {
                        if (!string.IsNullOrEmpty(imageUrl))
                        {
                            article.Photos.Add(new PhotoInArticle
                            {
                                Photo = imageUrl,
                                positsion = 0
                            });
                        }
                    }
                }

                // Add cover image URL (if provided)
                if (!string.IsNullOrEmpty(model.CoverImageUrl))
                {
                    article.Photos.Add(new PhotoInArticle
                    {
                        Photo = model.CoverImageUrl,
                        positsion = int.MinValue
                    });
                }

                // Add author (example with hardcoded AccountId)
                article.Authors.Add(new Author { AccountId = 1 });

                _context.Articles.Add(article);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Article saved with ID: {article.ArticleId}");
                Console.WriteLine($"Photos in article: {article.Photos.Count}");
                Console.WriteLine($"CoverImageUrl: {model.CoverImageUrl}");

                return Ok();
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine($"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
                return StatusCode(500, $"Database error: {dbEx.InnerException?.Message ?? dbEx.Message}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving article: {ex.Message}");
                return StatusCode(500, $"Error saving article: {ex.Message}");
            }
        }

        


    }
}






