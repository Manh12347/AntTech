using System.ComponentModel.DataAnnotations.Schema;

namespace AntTech.Models
{
    [Table("TagInArticle")]
    public class TagInArticle
    {
        // Khóa ngoại 1 (phần của khóa chính phức hợp)
        public int ArticleId { get; set; }

        // Khóa ngoại 2 (phần của khóa chính phức hợp)
        public int TagId { get; set; }

        // --- Thuộc tính điều hướng ---
        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }
    }

}
