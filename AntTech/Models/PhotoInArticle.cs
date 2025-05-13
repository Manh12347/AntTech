using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AntTech.Models
{
    [Table("PhotoInArticle")]
    public class PhotoInArticle
    {
        [Key]
        public int PhotoId { get; set; }

        // Khóa ngoại
        [ForeignKey("Article")]
        public int ArticleId { get; set; }
        public string Photo { get; set; } // varchar(MAX) -> string
        public int positsion { get; set; }

        // --- Thuộc tính điều hướng ---
        public virtual Article Article { get; set; }
    }
}
