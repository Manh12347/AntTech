using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AntTech.Models
{
    [Table("Notify")]
    public class Notify
    {
        [Key]
        public int NotifyId { get; set; }

        // Khóa ngoại
        [ForeignKey("Account")]
        public int AccountId { get; set; }

        [StringLength(1)]
        public string Catalogue { get; set; }

        [StringLength(255)]
        public string Detail { get; set; }

        public bool IsRead { get; set; } = false; // Giá trị mặc định

        // --- Thuộc tính điều hướng ---
        public virtual Account Account { get; set; }
    }
}
