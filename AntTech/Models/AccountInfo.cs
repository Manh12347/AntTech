using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Cần cho ForeignKey

namespace AntTech.Models
{
    [Table("AccountInfo")]
    public class AccountInfo
    {
        [Key] // Đánh dấu khóa chính
        [ForeignKey("Account")] // Khóa ngoại đến Account
        public int AccountId { get; set; }

        [StringLength(70)]
        public string RealName { get; set; }

        public string Avatar { get; set; } // varchar(MAX)

        // ===>>> THUỘC TÍNH EMAIL ĐÃ BỊ XÓA KHỎI ĐÂY <<<===
        // Vì email giờ được quản lý bởi bảng/entity AccountEmail

        public DateTime? CreatedDate { get; set; }

        public DateTime? DoB { get; set; }

        [StringLength(11)]
        public string PhoneNumber { get; set; }

        public bool Gender { get; set; } = false; // 0 Nam, 1 Nữ

        [Required(ErrorMessage = "UserID là bắt buộc.")]
        [StringLength(20)]
        // Không cần [Index(IsUnique=true)] nếu đã có UNIQUE trong SQL
        // Hoặc bạn có thể cấu hình bằng Fluent API trong DbContext
        public string UserId { get; set; }

        // --- Navigation Property ---
        // Liên kết ngược lại với Account (mối quan hệ 1-1)
        public virtual Account Account { get; set; }
    }
}