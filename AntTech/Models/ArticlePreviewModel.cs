using System; // Cần cho DateTime

namespace AntTech.Models // Hoặc AntTech.ViewModels nếu bạn có thư mục riêng
{
    /// <summary>
    /// ViewModel chứa thông tin tóm tắt của một bài viết để hiển thị trong danh sách hoặc lưới.
    /// Dữ liệu này thường được chuẩn bị sẵn bởi Controller.
    /// </summary>
    public class ArticlePreviewViewModel
    {
        /// <summary>
        /// ID của bài viết, dùng để tạo link.
        /// </summary>
        public int ArticleId { get; set; }

        /// <summary>
        /// Tiêu đề bài viết.
        /// </summary>
        public string Title { get; set; } = string.Empty; // Khởi tạo rỗng để tránh null

        /// <summary>
        /// Một đoạn trích dẫn ngắn hoặc mô tả tóm tắt nội dung.
        /// </summary>
        public string Snippet { get; set; } = string.Empty;

        /// <summary>
        /// URL của ảnh đại diện (thumbnail) cho bài viết.
        /// Controller nên gán URL ảnh mặc định nếu bài viết không có ảnh.
        /// </summary>
        public string ThumbnailUrl { get; set; } = "/images/placeholder-article.png"; // Gán mặc định sẵn

        /// <summary>
        /// Tên của category/tag chính được hiển thị.
        /// Controller nên gán giá trị mặc định nếu bài viết không có tag.
        /// </summary>
        public string PrimaryCategory { get; set; } = "Chưa phân loại"; // Gán mặc định sẵn

        /// <summary>
        /// Thời gian đọc ước tính (ví dụ: "5 phút đọc").
        /// Được tính toán bởi Controller.
        /// </summary>
        public string ReadingTimeEstimate { get; set; } = string.Empty;

        /// <summary>
        /// Tên hiển thị của tác giả.
        /// Controller nên gán giá trị mặc định nếu không tìm thấy tên.
        /// </summary>
        public string AuthorName { get; set; } = "Ẩn danh"; // Gán mặc định sẵn

        /// <summary>
        /// URL ảnh đại diện (avatar) của tác giả.
        /// Controller nên gán URL avatar mặc định nếu tác giả không có avatar.
        /// </summary>
        public string AuthorAvatarUrl { get; set; } = "/images/default-avatar.png"; // Gán mặc định sẵn

        /// <summary>
        /// Lượt xem bài viết. Có thể là null nếu không theo dõi hoặc bằng 0.
        /// </summary>
        public int? ViewCount { get; set; }

        /// <summary>
        /// Ngày đăng bài viết gốc. Có thể là null.
        /// </summary>
        public DateTime? PublishDate { get; set; }

        /// <summary>
        /// Ngày đăng bài viết đã được định dạng sẵn (ví dụ: "dd/MM/yyyy" hoặc "3 ngày trước").
        /// Được định dạng bởi Controller.
        /// </summary>
        public string PublishDateFormatted { get; set; } = string.Empty;

        /// <summary>
        /// URL thân thiện (slug) để truy cập bài viết (tùy chọn).
        /// </summary>
        // public string ArticleSlug { get; set; }
    }
}