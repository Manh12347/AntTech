using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AntTech.Models
{
    public class AccountInfo
    {
        [Key] // Đánh dấu khóa chính
        [ForeignKey("Account")] // Đánh dấu khóa ngoại, liên kết với thuộc tính Account bên dưới
        public int AccountId { get; set; }

        [StringLength(70)]
        public string RealName { get; set; }

        public string Avatar { get; set; } // varchar(MAX) -> string

        [StringLength(255)]
        public string Email { get; set; }

        public DateTime? CreatedDate { get; set; } // date -> DateTime? (cho phép null nếu cần)

        public DateTime? DoB { get; set; } // date -> DateTime?

        [StringLength(11)]
        public string PhoneNumber { get; set; }

        public bool Gender { get; set; } = false; // Giá trị mặc định

        [Required] // Vì có UNIQUE constraint
        [StringLength(20)]
        public string UserId { get; set; }

        // --- Thuộc tính điều hướng ---

        // Quan hệ 1-1 ngược lại với Account
        public virtual Account Account { get; set; }
    }
}
