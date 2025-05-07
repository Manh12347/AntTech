using System;
using System.Collections.Generic;

namespace AntTech.Models // Hoặc AntTech.ViewModels
{
    public class ArticleDetailViewModel
    {
        public int ArticleId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty; // Nội dung đầy đủ (HTML)

        public string AuthorName { get; set; } = "Ẩn danh";
        public string AuthorAvatarUrl { get; set; } = "/images/default-avatar.png"; // Cần có ảnh này
        public string AuthorProfileUrl { get; set; } = "#"; // Link đến trang cá nhân tác giả (nếu có)

        public DateTime? PublishDate { get; set; }
        public string PublishDateFormatted { get; set; } = string.Empty; // Ví dụ: "Ngày 06 tháng 05 năm 2024"
        public string ReadingTimeEstimate { get; set; } = string.Empty;

        public int LikeCount { get; set; }          // Like của BÀI VIẾT
        public int DislikeCount { get; set; }       // Dislike của BÀI VIẾT
        public int ViewCount { get; set; }          // View của BÀI VIẾT
        public bool IsArticleLikedByCurrentUser { get; set; } // Người dùng hiện tại đã like BÀI VIẾT này chưa

        public List<string> Tags { get; set; } = new List<string>(); // Danh sách tên các Tag
        public List<string> PhotoUrls { get; set; } = new List<string>(); // Danh sách URL các ảnh trong nội dung chính của bài viết

        // ===>>> DANH SÁCH BÌNH LUẬN (ĐÃ BAO GỒM REPLIES LỒNG NHAU) <<<===
        // Controller sẽ có trách nhiệm xây dựng cấu trúc này từ dữ liệu phẳng
        public List<CommentViewModel> Comments { get; set; } = new List<CommentViewModel>();

        // ID của người dùng đang xem (nếu đã đăng nhập), để tiện cho việc xử lý like/comment ở client
        public int? CurrentLoggedInAccountId { get; set; }

        // Các thuộc tính khác bạn có thể muốn thêm:
        // public List<ArticlePreviewViewModel> RelatedArticles { get; set; }
        // public string ArticleSlug { get; set; }
    }
}