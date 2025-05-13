using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Globalization; // Không còn cần trực tiếp ở đây nếu FormatDate đã là static và có using riêng
using AntTech.Models;      // Đảm bảo namespace chứa DbContext và ViewModels là đúng
// using AntTech.Data;     // Bỏ comment nếu DbContext ở đây

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

                // Lấy 4 bài viết nổi bật (view cao nhất TRONG THÁNG)
                viewModel.PopularArticles = await GetPopularArticlesByViewCountInCurrentMonth(4);
                Console.WriteLine($"[LOG] HomeController.Index: GetPopularArticlesByViewCountInCurrentMonth trả về {viewModel.PopularArticles?.Count ?? -1} bài viết.");

                // Lấy 4 bài viết mới nhất
                viewModel.RecentArticles = await GetRecentArticles(4);
                Console.WriteLine($"[LOG] HomeController.Index: GetRecentArticles trả về {viewModel.RecentArticles?.Count ?? -1} bài viết.");

                viewModel.PageTitle = "Trang Chủ AntTech";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!!!!!!!!!!!!!!!! LỖI TRONG HomeController.Index !!!!!!!!!!!!!!!!!");
                Console.WriteLine(ex.ToString());
                viewModel.PopularArticles = viewModel.PopularArticles ?? new List<ArticlePreviewViewModel>();
                viewModel.RecentArticles = viewModel.RecentArticles ?? new List<ArticlePreviewViewModel>();
                // Consider returning a proper error view
                // return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
            return View(viewModel);
        }

        // HÀM LẤY BÀI VIẾT NỔI BẬT THEO VIEW COUNT TRONG THÁNG HIỆN TẠI
        private async Task<List<ArticlePreviewViewModel>> GetPopularArticlesByViewCountInCurrentMonth(int count)
        {
            Console.WriteLine($"[LOG] GetPopularArticlesByViewCountInCurrentMonth: Bắt đầu truy vấn {count} bài viết có view cao nhất TRONG THÁNG...");
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
                                a.PublishDate >= firstDayOfMonth && // Lọc trong tháng hiện tại
                                a.PublishDate < firstDayOfNextMonth)
                    .OrderByDescending(a => a.ViewCount)
                    .ThenByDescending(a => a.PublishDate)
                    .Take(count)
                    .Select(MapArticleToPreviewViewModel()) // Sử dụng Expression
                    .ToListAsync();
                Console.WriteLine($"[LOG] GetPopularArticlesByViewCountInCurrentMonth: Truy vấn trả về {viewModels?.Count ?? -1} ViewModels.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI KHI LẤY POPULAR ARTICLES TRONG THÁNG: {ex.ToString()}");
                return new List<ArticlePreviewViewModel>();
            }
            return ProcessArticlePreviews(viewModels);
        }

        // HÀM LẤY BÀI VIẾT GẦN ĐÂY NHẤT
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
                    .Select(MapArticleToPreviewViewModel()) // Sử dụng lại Expression
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


        // EXPRESSION ĐỂ TÁI SỬ DỤNG LOGIC SELECT (ánh xạ từ Article sang ArticlePreviewViewModel)
        private static System.Linq.Expressions.Expression<Func<Article, ArticlePreviewViewModel>> MapArticleToPreviewViewModel()
        {
            return a => new ArticlePreviewViewModel
            {
                ArticleId = a.ArticleId,
                Title = a.Title ?? "Không có tiêu đề",
                Snippet = (a.Content ?? "").Length > 100 ? (a.Content ?? "").Substring(0, 100).Trim() + "..." : (a.Content ?? ""),
                ThumbnailUrl = a.Photos.Select(p => p.Photo).FirstOrDefault(),
                PrimaryCategory = a.ArticleTags.Select(at => at.Tag.TagName).FirstOrDefault(),
                ReadingTimeEstimate = CalculateReadingTime(a.Content), // Gọi hàm static
                AuthorName = a.Authors.Select(ath => ath.Account.AccountInfo.RealName).FirstOrDefault() ?? a.Authors.Select(ath => ath.Account.Username).FirstOrDefault(),
                AuthorAvatarUrl = a.Authors.Select(ath => ath.Account.AccountInfo.Avatar).FirstOrDefault(),
                ViewCount = a.ViewCount > 0 ? a.ViewCount : (int?)null,
                PublishDate = a.PublishDate,
                PublishDateFormatted = FormatDate(a.PublishDate) // Gọi hàm static
            };
        }

        // HÀM RIÊNG CHO XỬ LÝ HẬU KỲ VIEWMODELS (gán ảnh mặc định, etc.)
        private List<ArticlePreviewViewModel> ProcessArticlePreviews(List<ArticlePreviewViewModel> viewModels)
        {
            if (viewModels == null) return new List<ArticlePreviewViewModel>();

            // Console.WriteLine($"[LOG] ProcessArticlePreviews: Bắt đầu xử lý hậu kỳ cho {viewModels.Count} ViewModels..."); // Có thể bật lại nếu cần debug sâu
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
            // Console.WriteLine($"[LOG] ProcessArticlePreviews: Hoàn thành xử lý hậu kỳ.");
            return viewModels;
        }


        // HÀM TIỆN ÍCH STATIC
        private static string CalculateReadingTime(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return "1 phút đọc";
            var wordCount = content.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            var averageReadingSpeedWpm = 200;
            var readingTimeMinutes = Math.Ceiling(wordCount / (double)averageReadingSpeedWpm);
            return readingTimeMinutes <= 1 ? "1 phút đọc" : $"{readingTimeMinutes} phút đọc";
        }

        private static string FormatDate(DateTime? date) // Sử dụng System.Globalization trong using nếu định dạng phức tạp hơn
        {
            return date?.ToString("dd/MM/yyyy") ?? string.Empty;
        }

        // Action Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // Action Privacy
        public IActionResult Privacy()
        {
            return View();
        }
    }
}
