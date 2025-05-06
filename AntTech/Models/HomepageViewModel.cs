using System.Collections.Generic;

namespace AntTech.Models // Hoặc AntTech.ViewModels
{
    public class HomepageViewModel
    {
        public string PageTitle { get; set; } = "Trang Chủ"; // Tiêu đề trang (ví dụ)
        public List<ArticlePreviewViewModel> PopularArticles { get; set; } = new List<ArticlePreviewViewModel>(); // Danh sách bài viết nổi bật

        // Thêm các thuộc tính khác nếu trang chủ cần hiển thị thêm dữ liệu
        // public List<CategoryViewModel> FeaturedCategories { get; set; }
        // public BannerViewModel MainBanner { get; set; }
    }
}