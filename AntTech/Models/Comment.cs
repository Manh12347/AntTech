using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AntTech.Models
{
    [Table("Comment")]
    public class Comment
    {
        [Key]
        public int CommentId { get; set; }

        // Khóa ngoại
        [ForeignKey("Account")]
        public int AccountId { get; set; }

        // Khóa ngoại
        [ForeignKey("Article")]
        public int ArticleId { get; set; }

        public int LikeCount { get; set; } = 0;
        public int DislikeCount { get; set; } = 0;

        [StringLength(255)]
        public string Content { get; set; }

        public DateTime? CommentTime { get; set; } // datetime -> DateTime?

        // --- Thuộc tính điều hướng ---
        public virtual Account Account { get; set; }
        public virtual Article Article { get; set; }
    }

}
