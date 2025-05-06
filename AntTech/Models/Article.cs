using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AntTech.Models
{
    [Table("Article")]
    public class Article
    {
        [Key]
        public int ArticleId { get; set; }

        [StringLength(150)]
        public string Title { get; set; }

        public string Content { get; set; } // nvarchar(max) -> string

        public int LikeCount { get; set; } = 0;
        public int DislikeCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;

        [StringLength(1)]
        public string StatusSet { get; set; }

        public DateTime? PublishDate { get; set; } // datetime -> DateTime?
        public DateTime? UpdateDate { get; set; } // datetime -> DateTime?

        // --- Thuộc tính điều hướng ---
        // Quan hệ 1-Nhiều với PhotoInArticle
        public virtual ICollection<PhotoInArticle> Photos { get; set; } = new HashSet<PhotoInArticle>();

        // Quan hệ Nhiều-Nhiều với Account (qua Author)
        public virtual ICollection<Author> Authors { get; set; } = new HashSet<Author>();

        // Quan hệ Nhiều-Nhiều với Tag (qua TagInArticle)
        public virtual ICollection<TagInArticle> ArticleTags { get; set; } = new HashSet<TagInArticle>();

        // Quan hệ 1-Nhiều với Comment
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
    }
}
