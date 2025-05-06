using System.ComponentModel.DataAnnotations.Schema;

namespace AntTech.Models
{
    public class FollowerList
    {
        // Khóa ngoại 1 (phần của khóa chính phức hợp)
        public int AccountId { get; set; }

        // Khóa ngoại 2 (phần của khóa chính phức hợp)
        public int Follower { get; set; } // Đổi tên thành FollowerId để rõ ràng hơn là ID

        // --- Thuộc tính điều hướng ---
        [ForeignKey("AccountId")] // Liên kết với AccountId
        [InverseProperty("Followers")] // Liên kết với collection Followers trong Account
        public virtual Account FollowedAccount { get; set; } // Tài khoản được theo dõi

        [ForeignKey("Follower")] // Liên kết với Follower (FollowerId)
        [InverseProperty("Following")] // Liên kết với collection Following trong Account
        public virtual Account FollowerAccount { get; set; } // Tài khoản đi theo dõi
    }

}
