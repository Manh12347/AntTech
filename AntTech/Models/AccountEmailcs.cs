using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AntTech.Models
{
    [Table("AccountEmail")]
    // Entity này có thể được ánh xạ với bảng tên "AccountEmail" trong DB
    // nếu bạn dùng modelBuilder.Entity<AccountEmail>().ToTable("AccountEmail"); trong DbContext
    public class AccountEmail
    {
        [Key] // Đánh dấu accountId là khóa chính cho bảng này
        [ForeignKey("Account")] // Đánh dấu đây là khóa ngoại liên kết đến bảng Account
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ.")]
        [StringLength(255)]
        public string Email { get; set; }

        [StringLength(6)] // OTP thường có 6 ký tự
        public string CurrentOTP { get; set; } // Cho phép null nếu không có OTP nào đang hoạt động

        public DateTime? ExpiredOTP { get; set; } // Cho phép null nếu không có OTP hoặc OTP không có hạn

        // --- Navigation Property ---
        // Liên kết ngược lại với Account (mối quan hệ 1-1)
        public virtual Account Account { get; set; }
    }
}