using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.Security.Claims; // Cần cho User.FindFirstValue
using System.Diagnostics;    // Cần cho Activity
using AntTech.Models;      // Namespace chứa DbContext và các ViewModel/Model
// using AntTech.Data;     // Hoặc namespace chứa DbContext nếu khác

namespace AntTech.Controllers
{
    public class ArticleController : Controller
    {
        private readonly BlogWebsiteDbContext _context;

        public ArticleController(BlogWebsiteDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        // GET: Article (Danh sách bài viết)
        public async Task<IActionResult> Index(int page = 1, int pageSize = 10)
        {
            // (Logic cho trang danh sách bài viết có thể phức tạp hơn với tìm kiếm, lọc)
            // Tạm thời lấy các bài viết mới nhất
            try
            {
                var query = _context.Articles
                    .AsNoTracking()
                    .Where(a => a.StatusSet == "P")
                    .OrderByDescending(a => a.PublishDate);

                var totalArticles = await query.CountAsync();
                var articlesData = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(a => new ArticlePreviewViewModel // Sử dụng ViewModel xem trước
                    {
                        ArticleId = a.ArticleId,
                        Title = a.Title ?? "Không có tiêu đề",
                        Snippet = (a.Content ?? "").Length > 100 ? (a.Content ?? "").Substring(0, 100).Trim() + "..." : (a.Content ?? ""),
                        ThumbnailUrl = a.Photos.Select(p => p.Photo).FirstOrDefault() ?? "/images/placeholder-article.png",
                        PrimaryCategory = a.ArticleTags.Select(at => at.Tag.TagName).FirstOrDefault() ?? "Chưa phân loại",
                        ReadingTimeEstimate = CalculateReadingTime(a.Content),
                        AuthorName = a.Authors.Select(ath => ath.Account.AccountInfo.RealName).FirstOrDefault() ?? a.Authors.Select(ath => ath.Account.Username).FirstOrDefault() ?? "Ẩn danh",
                        AuthorAvatarUrl = a.Authors.Select(ath => ath.Account.AccountInfo.Avatar).FirstOrDefault() ?? "/images/default-avatar.png",
                        ViewCount = a.ViewCount > 0 ? a.ViewCount : (int?)null,
                        PublishDate = a.PublishDate,
                        PublishDateFormatted = FormatDateShort(a.PublishDate)
                    })
                    .ToListAsync();

                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = (int)Math.Ceiling(totalArticles / (double)pageSize);

                return View(articlesData);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI TRONG ArticleController.Index: {ex.ToString()}");
                return View(new List<ArticlePreviewViewModel>());
            }
        }


        // GET: Article/Details/{id}
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound("ID bài viết không được cung cấp.");
            }

            var currentAccountId = GetCurrentAccountId(); // Lấy ID người dùng đang đăng nhập

            try
            {
                var articleEntity = await _context.Articles
                    .AsNoTracking()
                    .Include(a => a.Photos)
                    .Include(a => a.ArticleTags)
                        .ThenInclude(at => at.Tag)
                    .Include(a => a.Authors)
                        .ThenInclude(ath => ath.Account)
                            .ThenInclude(acc => acc.AccountInfo)
                    // Tải Comments và dữ liệu liên quan của Comments
                    .Include(a => a.Comments)
                        .ThenInclude(c => c.Account)           // Người đăng comment
                            .ThenInclude(acc => acc.AccountInfo) // Thông tin người đăng comment
                    .Include(a => a.Comments)
                        .ThenInclude(c => c.CommentLikes)       // Lượt like của comment
                    .Include(a => a.Comments)                   // Cần tải replies cho từng comment
                        .ThenInclude(c => c.Replies)            // Danh sách replies của comment
                            .ThenInclude(r => r.Account)        // Người đăng reply
                                .ThenInclude(acc => acc.AccountInfo) // Thông tin người đăng reply
                    .Include(a => a.Comments)
                        .ThenInclude(c => c.Replies)
                            .ThenInclude(r => r.CommentLikes) // Lượt like của reply
                    .FirstOrDefaultAsync(m => m.ArticleId == id && m.StatusSet == "P");

                if (articleEntity == null)
                {
                    return NotFound($"Không tìm thấy bài viết với ID {id} hoặc bài viết không công khai.");
                }

                // Xây dựng cây bình luận
                var commentTree = BuildCommentTree(articleEntity.Comments, currentAccountId);

                var viewModel = new ArticleDetailViewModel
                {
                    ArticleId = articleEntity.ArticleId,
                    Title = articleEntity.Title ?? "Không có tiêu đề",
                    Content = articleEntity.Content ?? string.Empty,
                    LikeCount = articleEntity.LikeCount,
                    DislikeCount = articleEntity.DislikeCount,
                    ViewCount = articleEntity.ViewCount,
                    PublishDate = articleEntity.PublishDate,
                    PublishDateFormatted = FormatDateLong(articleEntity.PublishDate),
                    ReadingTimeEstimate = CalculateReadingTime(articleEntity.Content),
                    AuthorName = articleEntity.Authors?.FirstOrDefault()?.Account?.AccountInfo?.RealName
                                 ?? articleEntity.Authors?.FirstOrDefault()?.Account?.Username
                                 ?? "Ẩn danh",
                    AuthorAvatarUrl = articleEntity.Authors?.FirstOrDefault()?.Account?.AccountInfo?.Avatar
                                      ?? "/images/default-avatar.png",
                    Tags = articleEntity.ArticleTags?.Select(at => at.Tag?.TagName).Where(tn => tn != null).ToList()
                           ?? new List<string>(),
                    PhotoUrls = articleEntity.Photos?.Select(p => p.Photo).Where(purl => purl != null).ToList()
                                ?? new List<string>(),
                    Comments = commentTree, // Gán cây bình luận đã xử lý
                    CurrentLoggedInAccountId = currentAccountId
                    // IsArticleLikedByCurrentUser = ... (Cần logic lấy từ bảng ArticleLike nếu có)
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LỖI TRONG ArticleController.Details (ID: {id}): {ex.ToString()}");
                return View("Error", new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
            }
        }


        // --- HÀM TIỆN ÍCH ---

        private List<CommentViewModel> BuildCommentTree(IEnumerable<Comment> allCommentsFlatList, int? currentAccountId)
        {
            if (allCommentsFlatList == null || !allCommentsFlatList.Any())
            {
                return new List<CommentViewModel>();
            }

            // Map tất cả comments thành ViewModels trước
            var commentViewModels = allCommentsFlatList
                .Select(c => MapCommentToViewModel(c, currentAccountId))
                .Where(cvm => cvm != null) // Loại bỏ null nếu có lỗi map
                .ToList();

            // Tạo lookup để dễ dàng tìm comment cha
            var commentLookup = commentViewModels.ToDictionary(c => c.CommentId);
            var rootComments = new List<CommentViewModel>();

            foreach (var cvm in commentViewModels)
            {
                if (cvm.ParentCommentId.HasValue && commentLookup.TryGetValue(cvm.ParentCommentId.Value, out var parentCvm))
                {
                    parentCvm.Replies.Add(cvm); // Thêm reply vào comment cha
                }
                else if (!cvm.ParentCommentId.HasValue) // Chỉ thêm comment gốc vào rootComments
                {
                    rootComments.Add(cvm);
                }
            }

            // Sắp xếp replies trong mỗi comment gốc
            foreach (var rootComment in rootComments)
            {
                rootComment.Replies = rootComment.Replies.OrderBy(r => r.CommentTime).ToList();
            }

            // Sắp xếp comment gốc (ví dụ: mới nhất lên đầu)
            return rootComments.OrderByDescending(c => c.CommentTime).ToList();
        }

        private CommentViewModel MapCommentToViewModel(Comment commentEntity, int? currentAccountId)
        {
            if (commentEntity == null) return null;

            return new CommentViewModel
            {
                CommentId = commentEntity.CommentId,
                ParentCommentId = commentEntity.ParentCommentId,
                UserName = commentEntity.Account?.AccountInfo?.RealName ?? commentEntity.Account?.Username ?? "Người dùng ẩn danh",
                UserAvatarUrl = commentEntity.Account?.AccountInfo?.Avatar ?? "/images/default-avatar.png",
                Content = commentEntity.Content ?? string.Empty,
                CommentTime = commentEntity.CommentTime,
                CommentTimeFormatted = FormatCommentTime(commentEntity.CommentTime),
                LikeCount = commentEntity.CommentLikes?.Count() ?? commentEntity.LikeCount, // Ưu tiên đếm từ bảng CommentLike
                IsLikedByCurrentUser = currentAccountId.HasValue && (commentEntity.CommentLikes?.Any(cl => cl.AccountId == currentAccountId.Value) ?? false)
            };
        }

        private static string CalculateReadingTime(string content)
        {
            if (string.IsNullOrWhiteSpace(content)) return "1 phút đọc";
            var wordCount = content.Split(new[] { ' ', '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length;
            var averageReadingSpeedWpm = 200;
            var readingTimeMinutes = Math.Ceiling(wordCount / (double)averageReadingSpeedWpm);
            return readingTimeMinutes <= 1 ? "1 phút đọc" : $"{readingTimeMinutes} phút đọc";
        }

        private static string FormatDateShort(DateTime? date)
        {
            return date?.ToString("dd/MM/yyyy") ?? string.Empty;
        }

        private static string FormatDateLong(DateTime? date)
        {
            if (!date.HasValue) return "Chưa xác định";
            CultureInfo viVn = new CultureInfo("vi-VN");
            return $"Ngày {date.Value.ToString("dd 'tháng' MM 'năm' yyyy", viVn)}";
        }

        private static string FormatCommentTime(DateTime? commentTime)
        {
            if (!commentTime.HasValue) return "Vừa xong";
            var timeSpan = DateTime.Now - commentTime.Value;
            if (timeSpan.TotalSeconds < 60) return $"{(int)timeSpan.TotalSeconds} giây trước";
            if (timeSpan.TotalMinutes < 60) return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 24) return $"{(int)timeSpan.TotalHours} giờ trước";
            if (timeSpan.TotalDays < 7) return $"{(int)timeSpan.TotalDays} ngày trước";
            CultureInfo viVn = new CultureInfo("vi-VN");
            return commentTime.Value.ToString("dd MMM, yyyy 'lúc' HH:mm", viVn);
        }

        private int? GetCurrentAccountId() // Hàm này cần được điều chỉnh tùy theo cách bạn quản lý session/authentication
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (int.TryParse(userIdString, out int accountId))
                {
                    return accountId;
                }
            }
            return null;
        }

        // Action Error
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}