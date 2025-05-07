using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AntTech.Models
{
    [Table("Comment")]
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        [Required]
        public int AccountId { get; set; }

        [Required]
        public int ArticleId { get; set; }

        public int LikeCount { get; set; } = 0; // Sẽ được cập nhật dựa trên bảng CommentLike hoặc trigger
        public int DislikeCount { get; set; } = 0;

        [Required]
        [StringLength(1000)]
        public string Content { get; set; }

        public DateTime CommentTime { get; set; } = DateTime.Now;

        public int? ParentCommentId { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; }

        [ForeignKey("ParentCommentId")]
        public virtual Comment ParentComment { get; set; }
        public virtual ICollection<Comment> Replies { get; set; } = new HashSet<Comment>();

        // ===>>> THÊM COLLECTION CHO COMMENTLIKES <<<===
        public virtual ICollection<CommentLike> CommentLikes { get; set; } = new HashSet<CommentLike>();
    }
}