using System.ComponentModel.DataAnnotations; // Cần cho các Data Annotations

namespace AntTech.Models // Hoặc AntTech.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email hoặc Tên đăng nhập.")]
        [Display(Name = "Email hoặc Tên đăng nhập")]
        // Bạn có thể dùng [EmailAddress] nếu chỉ cho phép đăng nhập bằng Email,
        // hoặc bỏ qua nếu cho phép cả Username.
        public string EmailOrUsername { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)] // Giúp trình duyệt hiển thị dưới dạng input password
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập?")]
        public bool RememberMe { get; set; }

        public string ReturnUrl { get; set; }
    }
}