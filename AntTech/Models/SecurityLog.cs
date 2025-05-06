using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AntTech.Models
{
    public class SecurityLog
    {
        [Key]
        public int ActionId { get; set; }

        // Khóa ngoại
        [ForeignKey("Account")]
        public int AccountId { get; set; }

        public DateTime? TimeRecord { get; set; } // datetime -> DateTime?

        [StringLength(1)]
        public string ActionRecord { get; set; } = "L"; // Giá trị mặc định

        [StringLength(255)]
        public string Detail { get; set; }

        [StringLength(15)]
        public string IpAddress { get; set; }

        // --- Thuộc tính điều hướng ---
        public virtual Account Account { get; set; }
    }
}
