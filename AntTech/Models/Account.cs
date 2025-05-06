using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AntTech.Models
{
    public class Account
    {
        [Key] // Đánh dấu khóa chính
        public int AccountId { get; set; }

        [Required] // Vì có UNIQUE constraint
        [StringLength(50)]
        public string Username { get; set; }

        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(1)]
        public string AccountRole { get; set; } = "U"; // Giá trị mặc định

        // --- Thuộc tính điều hướng (Navigation Properties) ---

        // Quan hệ 1-1 với AccountInfo
        public virtual AccountInfo AccountInfo { get; set; }

        // Quan hệ 1-Nhiều với SecurityLog
        public virtual ICollection<SecurityLog> SecurityLogs { get; set; } = new HashSet<SecurityLog>();

        // Quan hệ 1-Nhiều với Notify
        public virtual ICollection<Notify> Notifications { get; set; } = new HashSet<Notify>();

        // Quan hệ Nhiều-Nhiều với Account (qua FollowerList) - Người mình theo dõi
        [InverseProperty("FollowerAccount")] // Chỉ rõ FK nào trong FollowerList ứng với collection này
        public virtual ICollection<FollowerList> Following { get; set; } = new HashSet<FollowerList>();

        // Quan hệ Nhiều-Nhiều với Account (qua FollowerList) - Người theo dõi mình
        [InverseProperty("FollowedAccount")] // Chỉ rõ FK nào trong FollowerList ứng với collection này
        public virtual ICollection<FollowerList> Followers { get; set; } = new HashSet<FollowerList>();

        // Quan hệ Nhiều-Nhiều với Article (qua Author)
        public virtual ICollection<Author> AuthoredArticles { get; set; } = new HashSet<Author>();

        // Quan hệ 1-Nhiều với Comment
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();

        // Quan hệ Nhiều-Nhiều với Tag (qua PreferTag)
        public virtual ICollection<PreferTag> PreferredTags { get; set; } = new HashSet<PreferTag>();
    }
}
