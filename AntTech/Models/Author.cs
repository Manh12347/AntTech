using System.ComponentModel.DataAnnotations.Schema;

namespace AntTech.Models
{
    public class Author
    {
        // Khóa ngoại 1 (phần của khóa chính phức hợp)
        public int ArticleId { get; set; }

        // Khóa ngoại 2 (phần của khóa chính phức hợp)
        public int AccountId { get; set; }

        // --- Thuộc tính điều hướng ---
        [ForeignKey("ArticleId")]
        public virtual Article Article { get; set; }

        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }
    }
}
