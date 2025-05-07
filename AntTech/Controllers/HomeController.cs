using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Diagnostics;
using System.Globalization;

// *** ĐẢM BẢO USING NÀY ĐÚNG VỚI NAMESPACE CHỨA DBCONTEXT VÀ VIEWMODEL ***
using AntTech.Models;

namespace AntTech.Controllers
{
    public class HomeController : Controller
    {
        // *** Đảm bảo tên class DbContext này đúng: BlogWebsiteDbContext ***
        private readonly BlogWebsiteDbContext _context;

        public HomeController(BlogWebsiteDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // *** ACTION INDEX ***
        public async Task<IActionResult> Index()
        {
            var viewModel = new HomepageViewModel(); // Tạo ViewModel cho trang chủ

            try
            {
                Console.WriteLine("[LOG] HomeController.Index: Bắt đầu gọi GetPopularArticles...");

                viewModel.PopularArticles = await GetPopularArticles(4); // Lấy 4 bài viết

                Console.WriteLine($"[LOG] HomeController.Index: GetPopularArticles trả về {viewModel.PopularArticles?.Count ?? -1} bài viết (null nếu -1).");

                // Lấy thêm dữ liệu khác cho trang chủ nếu cần
                // viewModel.PageTitle = "Chào mừng đến AntTech";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!!!!!!!!!!!!!!!! LỖI TRONG HomeController.Index !!!!!!!!!!!!!!!!!");
                Console.WriteLine(ex.ToString());
                Console.WriteLine($"!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!");
                // viewModel.PopularArticles đã được khởi tạo là list rỗng
                // return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }

            return View(viewModel); // Truyền HomepageViewModel tới View
        }

        // *** HÀM LẤY DỮ LIỆU ***
        private async Task<List<ArticlePreviewViewModel>> GetPopularArticles(int count)
        {
            Console.WriteLine($"[LOG] GetPopularArticles: Bắt đầu truy vấn {count} bài viết...");

            if (_context?.Articles == null) // Kiểm tra cả context và DbSet
            {
                Console.WriteLine("[ERROR] GetPopularArticles: _context hoặc _context.Articles bị NULL!");
                return new List<ArticlePreviewViewModel>();
            }

            List<ArticlePreviewViewModel> viewModels = null;

            try
            {
                // Truy vấn và map trực tiếp sang ViewModel
                viewModels = await _context.Articles
                    .AsNoTracking()
                    .Where(a => a.StatusSet == "P") // Điều kiện StatusSet
                    .OrderByDescending(a => a.PublishDate)
                    .Take(count)
                    .Select(a => new ArticlePreviewViewModel
                    {
                        ArticleId = a.ArticleId,
                        Title = a.Title ?? "Không có tiêu đề",
                        Snippet = (a.Content ?? "").Length > 100 ? (a.Content ?? "").Substring(0, 100).Trim() + "..." : (a.Content ?? ""),
                        ThumbnailUrl = a.Photos.Select(p => p.Photo).FirstOrDefault(),
                        PrimaryCategory = a.ArticleTags.Select(at => at.Tag.TagName).FirstOrDefault(),
                        // ===>>> GỌI HÀM STATIC <<<===
                        ReadingTimeEstimate = CalculateReadingTime(a.Content),
                        AuthorName = a.Authors.Select(ath => ath.Account.AccountInfo.RealName).FirstOrDefault() ?? a.Authors.Select(ath => ath.Account.Username).FirstOrDefault(),
                        AuthorAvatarUrl = a.Authors.Select(ath => ath.Account.AccountInfo.Avatar).FirstOrDefault(),
                        ViewCount = a.ViewCount > 0 ? a.ViewCount : (int?)null,
                        PublishDate = a.PublishDate,
                        // ===>>> GỌI HÀM STATIC <<<===
                        PublishDateFormatted = FormatDate(a.PublishDate)
                    })
                    .ToListAsync();

                Console.WriteLine($"[LOG] GetPopularArticles: Truy vấn DB và Select trả về {viewModels?.Count ?? -1} ViewModels.");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"!!!!!!!!!!!!!!!!! LỖI TRONG GetPopularArticles KHI TRUY VẤN/SELECT !!!!!!!!!!!!!!!!!");
                Console.WriteLine(ex.ToString());
                return new List<ArticlePreviewViewModel>();
            }

            // Xử lý hậu kỳ
            if (viewModels != null)
            {
                Console.WriteLine($"[LOG] GetPopularArticles: Bắt đầu xử lý hậu kỳ cho {viewModels.Count} ViewModels...");
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
                Console.WriteLine($"[LOG] GetPopularArticles: Hoàn thành xử lý hậu kỳ.");
            }
            else
            {
                Console.WriteLine("[WARN] GetPopularArticles: viewModels bị null sau truy vấn!");
                viewModels = new List<ArticlePreviewViewModel>();
            }
            return viewModels;
        }

        // *** HÀM TIỆN ÍCH STATIC ***
        // ===>>> THÊM static <<<===
        private static string CalculateReadingTime(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return "1 phút đọc";
            var wordCount = content.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            var averageReadingSpeedWpm = 200;
            var readingTimeMinutes = Math.Ceiling(wordCount / (double)averageReadingSpeedWpm);
            return readingTimeMinutes <= 1 ? "1 phút đọc" : $"{readingTimeMinutes} phút đọc";
        }

        // ===>>> THÊM static <<<===
        private static string FormatDate(DateTime? date)
        {
            // Định dạng ngắn gọn (ví dụ: 06/05/2024)
            return date?.ToString("dd/MM/yyyy") ?? string.Empty;
        }

        // Có thể tạo thêm hàm static FormatDateLong nếu cần ở chỗ khác
        // private static string FormatDateLong(DateTime? date) { ... }

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

//using AntTech.Models;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Diagnostics;

//namespace AntTech.Controllers
//{
//    public class HomeController : Controller
//    {
//        private readonly ILogger<HomeController> _logger;
//        private readonly BlogWebsiteContext _context;

//        public HomeController(ILogger<HomeController> logger, BlogWebsiteContext context)
//        {
//            _logger = logger;
//            _context = context;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var ads = await _context.Ads
//                .Where(a => a.IsActive &&
//                          (a.EndDate == null || a.EndDate >= DateTime.Now) &&
//                           a.StartDate <= DateTime.Now)
//                .ToListAsync();

//            // Phân loại quảng cáo để sử dụng trong các view khác nhau
//            ViewBag.SidebarAds = ads.Where(a => a.AdType == "sidebar").ToList();
//            ViewBag.BannerAds = ads.Where(a => a.AdType == "banner").ToList();

//            return View(ads);
//        }

//        public IActionResult Privacy()
//        {
//            return View();
//        }

//        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
//        public IActionResult Error()
//        {
//            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
//        }

//        [HttpPost]
//        public async Task<IActionResult> TrackAdClick(int adId)
//        {
//            var ad = await _context.Ads.FindAsync(adId);
//            if (ad != null)
//            {
//                ad.ClickCount++;
//                await _context.SaveChangesAsync();
//            }
//            return Ok();
//        }
//    }
//}