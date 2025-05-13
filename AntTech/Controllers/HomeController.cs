using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AntTech.Models;
using System.Diagnostics;

namespace AntTech.Controllers
{
    public class HomeController : Controller
    {
        private readonly BlogWebsiteDbContext _context;

        public HomeController(BlogWebsiteDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new HomepageViewModel();

            try
            {
                Console.WriteLine("[LOG] HomeController.Index: Bắt đầu lấy dữ liệu...");
                viewModel.PopularArticles = await GetPopularArticlesByViewCountInCurrentMonth(4);
                viewModel.RecentArticles = await GetRecentArticles(4);
                viewModel.PageTitle = "Trang Chủ AntTech";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!!!!!!!!!!!!!!!! LỖI TRONG HomeController.Index !!!!!!!!!!!!!!!!!");
                Console.WriteLine(ex.ToString());
                viewModel.PopularArticles = viewModel.PopularArticles ?? new List<ArticlePreviewViewModel>();
                viewModel.RecentArticles = viewModel.RecentArticles ?? new List<ArticlePreviewViewModel>();
            }
            return View(viewModel);
        }

        private async Task<List<ArticlePreviewViewModel>> GetPopularArticlesByViewCountInCurrentMonth(int count)
        {
            Console.WriteLine($"[LOG] GetPopularArticlesByViewCountInCurrentMonth: Bắt đầu truy vấn {count} bài viết...");
            if (_context?.Articles == null)
            {
                Console.WriteLine("[ERROR] GetPopularArticlesByViewCountInCurrentMonth: _context hoặc _context.Articles bị NULL!");
                return new List<ArticlePreviewViewModel>();
            }

            DateTime now = DateTime.Now;
            DateTime firstDayOfMonth = new DateTime(now.Year, now.Month, 1);
            DateTime firstDayOfNextMonth = firstDayOfMonth.AddMonths(1);

            List<ArticlePreviewViewModel> viewModels;
            try
            {
                viewModels = await _context.Articles
                    .AsNoTracking()
                    .Where(a => a.StatusSet == "P" &&
                                a.PublishDate >= firstDayOfMonth &&
                                a.PublishDate < firstDayOfNextMonth)
                    .OrderByDescending(a => a.ViewCount)
                    .ThenByDescending(a => a.PublishDate)
                    .Select(MapArticleToPreviewViewModel())
                    .ToListAsync();
                Console.WriteLine($"[LOG] GetPopularArticlesByViewCountInCurrentMonth: Truy vấn trả về {viewModels?.Count ?? -1} ViewModels.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI KHI LẤY POPULAR ARTICLES: {ex.ToString()}");
                return new List<ArticlePreviewViewModel>();
            }
            return ProcessArticlePreviews(viewModels);
        }

        private async Task<List<ArticlePreviewViewModel>> GetRecentArticles(int count)
        {
            Console.WriteLine($"[LOG] GetRecentArticles: Bắt đầu truy vấn {count} bài viết mới nhất...");
            if (_context?.Articles == null)
            {
                Console.WriteLine("[ERROR] GetRecentArticles: _context hoặc _context.Articles bị NULL!");
                return new List<ArticlePreviewViewModel>();
            }

            List<ArticlePreviewViewModel> viewModels;
            try
            {
                viewModels = await _context.Articles
                    .AsNoTracking()
                    .Where(a => a.StatusSet == "P")
                    .OrderByDescending(a => a.PublishDate)
                    .Take(count)
                    .Select(MapArticleToPreviewViewModel())
                    .ToListAsync();
                Console.WriteLine($"[LOG] GetRecentArticles: Truy vấn trả về {viewModels?.Count ?? -1} ViewModels.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI KHI LẤY RECENT ARTICLES: {ex.ToString()}");
                return new List<ArticlePreviewViewModel>();
            }
            return ProcessArticlePreviews(viewModels);
        }

        private static System.Linq.Expressions.Expression<Func<Article, ArticlePreviewViewModel>> MapArticleToPreviewViewModel()
        {
            return a => new ArticlePreviewViewModel
            {
                ArticleId = a.ArticleId,
                Title = a.Title ?? "Không có tiêu đề",
                Snippet = FilterContent(a.Content), // Apply custom text filter
                ThumbnailUrl = a.Photos.Where(p => p.positsion == int.MinValue).Select(p => p.Photo).FirstOrDefault() ?? // Prefer cover image
                               a.Photos.Select(p => p.Photo).FirstOrDefault(),
                PrimaryCategory = a.ArticleTags.Select(at => at.Tag.TagName).FirstOrDefault(),
                ReadingTimeEstimate = CalculateReadingTime(a.Content),
                AuthorName = a.Authors.Select(ath => ath.Account.AccountInfo.RealName).FirstOrDefault() ??
                             a.Authors.Select(ath => ath.Account.Username).FirstOrDefault(),
                AuthorAvatarUrl = a.Authors.Select(ath => ath.Account.AccountInfo.Avatar).FirstOrDefault(),
                ViewCount = a.ViewCount > 0 ? a.ViewCount : (int?)null,
                PublishDate = a.PublishDate,
                PublishDateFormatted = FormatDate(a.PublishDate)
            };
        }

        // Custom text filter to remove ![alt](url) markdown
        private static string FilterContent(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return "";
            }

            // Remove ![alt](url) patterns
            string filteredContent = Regex.Replace(content, @"!\[[^\]]*\]\([^)]+\)", "").Trim();

            // Truncate to 100 characters for Snippet, adding ellipsis if needed
            if (filteredContent.Length > 100)
            {
                filteredContent = filteredContent.Substring(0, 100).Trim() + "...";
            }

            Console.WriteLine($"[LOG] FilterContent: Original='{content}', Filtered='{filteredContent}'");
            return filteredContent;
        }

        private List<ArticlePreviewViewModel> ProcessArticlePreviews(List<ArticlePreviewViewModel> viewModels)
        {
            if (viewModels == null) return new List<ArticlePreviewViewModel>();
            foreach (var article in viewModels)
            {
                if (string.IsNullOrWhiteSpace(article.ThumbnailUrl))
                {
                    article.ThumbnailUrl = "/images/placeholder-article.png";
                }
                if (string.IsNullOrWhiteSpace(article.AuthorAvatarUrl))
                {
                    article.AuthorAvatarUrl = "/images/default-avatar.png";
                }
                if (string.IsNullOrWhiteSpace(article.PrimaryCategory))
                {
                    article.PrimaryCategory = "Chưa phân loại";
                }
                if (string.IsNullOrWhiteSpace(article.AuthorName))
                {
                    article.AuthorName = "Ẩn danh";
                }
            }
            return viewModels;
        }

        private static string CalculateReadingTime(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return "1 phút đọc";
            var wordCount = content.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            var averageReadingSpeedWpm = 200;
            var readingTimeMinutes = Math.Ceiling(wordCount / (double)averageReadingSpeedWpm);
            return readingTimeMinutes <= 1 ? "1 phút đọc" : $"{readingTimeMinutes} phút đọc";
        }

        private static string FormatDate(DateTime? date)
        {
            return date?.ToString("dd/MM/yyyy") ?? string.Empty;
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Privacy()
        {
            return View();
        }
    }
}
