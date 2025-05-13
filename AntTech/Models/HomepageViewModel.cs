using System.Collections.Generic;
using AntTech.Models; // Đảm bảo namespace ArticlePreviewViewModel là đúng

namespace AntTech.Models
{
    public class HomepageViewModel
    {
        public string PageTitle { get; set; } = "Trang Chủ";
        public List<ArticlePreviewViewModel> PopularArticles { get; set; } = new List<ArticlePreviewViewModel>(); // Bài viết nổi bật (view cao trong tháng)
        public List<ArticlePreviewViewModel> RecentArticles { get; set; } = new List<ArticlePreviewViewModel>();  // Bài viết mới nhất
        public List<ArticlePreviewViewModel> LatestArticles { get; set; }
        public Ads BannerAd { get; set; }
        // Thêm các thuộc tính khác nếu cần
    }
}