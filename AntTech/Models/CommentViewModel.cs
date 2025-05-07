using System;
using System.Collections.Generic;

namespace AntTech.Models // Hoặc AntTech.ViewModels
{

    public class CommentViewModel
    {
        public int CommentId { get; set; }
        public int? ParentCommentId { get; set; } // ID của comment cha, nếu đây là reply
        public string UserName { get; set; } = "Người dùng ẩn danh";
        public string UserAvatarUrl { get; set; } = "/images/default-avatar.png";
        public string Content { get; set; } = string.Empty;
        public DateTime CommentTime { get; set; } // Nên giữ DateTime gốc để sort/logic khác
        public string CommentTimeFormatted { get; set; } = string.Empty; // Chuỗi hiển thị (ví dụ "5 phút trước")
        public int LikeCount { get; set; }
        public bool IsLikedByCurrentUser { get; set; } // Controller sẽ set giá trị này

        // Danh sách các replies cho bình luận này (để tạo cấu trúc lồng nhau)
        public List<CommentViewModel> Replies { get; set; } = new List<CommentViewModel>();
    }
}