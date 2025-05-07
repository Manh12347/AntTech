using System.ComponentModel.DataAnnotations;

namespace AntTech.Models // Hoặc namespace chứa ViewModel của bạn
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập Email hoặc Tên đăng nhập.")]
        [Display(Name = "Email hoặc Tên đăng nhập")]
        public string Email { get; set; } // Có thể là Email hoặc Username tùy bạn thiết kế logic đăng nhập

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }

        [Display(Name = "Ghi nhớ đăng nhập?")]
        public bool RememberMe { get; set; }

        // Thuộc tính ReturnUrl này thường được xử lý bởi Controller khi redirect,
        // nhưng bạn có thể thêm vào đây nếu Partial View cần nó một cách tường minh
        // public string ReturnUrl { get; set; }
    }
}