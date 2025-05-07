using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AntTech.Models
{
    [Table("Account")]
    public class Account
    {
        [Key]
        public int AccountId { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [StringLength(255)]
        public string PasswordHash { get; set; }

        [StringLength(1)]
        public string AccountRole { get; set; } = "U";

        // --- Navigation Properties ---
        public virtual AccountInfo AccountInfo { get; set; } // Giữ nguyên

        // ===>>> THÊM NAVIGATION PROPERTY NÀY <<<===
        public virtual AccountEmail AccountEmail { get; set; } // Mối quan hệ 1-1 với AccountEmail

        public virtual ICollection<SecurityLog> SecurityLogs { get; set; } = new HashSet<SecurityLog>();
        public virtual ICollection<Notify> Notifications { get; set; } = new HashSet<Notify>();
        public virtual ICollection<FollowerList> Following { get; set; } = new HashSet<FollowerList>(); // Những người mình đang theo dõi
        public virtual ICollection<FollowerList> Followers { get; set; } = new HashSet<FollowerList>(); // Những người đang theo dõi mình
        public virtual ICollection<Author> AuthoredArticles { get; set; } = new HashSet<Author>();
        public virtual ICollection<Comment> Comments { get; set; } = new HashSet<Comment>();
        public virtual ICollection<PreferTag> PreferredTags { get; set; } = new HashSet<PreferTag>();
        // public virtual ICollection<CommentLike> LikedComments { get; set; } = new HashSet<CommentLike>(); // Nếu có
    }
}