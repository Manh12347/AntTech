using System.ComponentModel.DataAnnotations.Schema;

namespace AntTech.Models
{
    [Table("PreferTag")]
    public class PreferTag
    {
        // Khóa ngoại 1 (phần của khóa chính phức hợp)
        public int AccountId { get; set; }

        // Khóa ngoại 2 (phần của khóa chính phức hợp)
        public int TagId { get; set; }

        public int ViewCount { get; set; } = 0;

        // --- Thuộc tính điều hướng ---
        [ForeignKey("AccountId")]
        public virtual Account Account { get; set; }

        [ForeignKey("TagId")]
        public virtual Tag Tag { get; set; }
    }
}
