using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Collections.Generic;
using AntTech.Models;

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

        foreach (var imageUrl in model.ImageUrls)
        {
            int position = model.Content.IndexOf(imageUrl);
            _photos.Add(new PhotoInArticle
            {
                Photo = imageUrl,
                Position = position
            });
        }

        return RedirectToAction("SavePost");
    }

    [HttpPost]
    public async Task<IActionResult> ConfirmSave([FromBody] ConfirmSaveDto model)
    {
        try
        {
            // Create the article
            var article = new Article
            {
                Title = model.Title,
                Content = model.Content,
                StatusSet = "P",
                PublishDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                LikeCount = 0,
                DislikeCount = 0,
                ViewCount = 0
            };

            // Add photos
            foreach (var imageUrl in model.ImageUrls)
            {
                int position = model.Content.IndexOf(imageUrl);
                article.Photos.Add(new PhotoInArticle
                {
                    Photo = imageUrl,
                    Position = position
                });
            }

            // Add tags
            if (model.CategoryIds != null)
            {
                foreach (var tagId in model.CategoryIds)
                {
                    article.ArticleTags.Add(new TagInArticle
                    {
                        TagId = tagId
                    });
                }
            }

            // Add author (AccountId = 1)
            article.Authors.Add(new Author
            {
                AccountId = 1 // Hardcoded as per request
            });

            // Save to database
            _context.Articles.Add(article);
            await _context.SaveChangesAsync();

            // Clear static draft data after successful save
            _draftArticle = null;
            _photos.Clear();
            _tags.Clear();
            _authors.Clear();

            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
            return StatusCode(500, "Error saving article");
        }
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
    }
}



[Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile image)
        {
            if (image != null && image.Length > 0)
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads", image.FileName);
                var directory = Path.GetDirectoryName(filePath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }
                var url = Url.Content($"~/uploads/{image.FileName}");
                return Ok(new { url });
            }
            return BadRequest();
        }
    }

