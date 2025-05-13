using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity; // << QUAN TRỌNG: Cho UserManager, SignInManager
using System.Threading.Tasks;
using AntTech.Models; // Namespace chứa LoginViewModel và lớp User (Account) của bạn
using Microsoft.AspNetCore.Authorization; // Cho [AllowAnonymous], [Authorize]
using Microsoft.AspNetCore.Authentication; // Cho HttpContext.SignOutAsync (nếu không dùng SignInManager)
using System.Security.Claims; // Nếu bạn muốn thêm Claims tùy chỉnh
using Microsoft.Extensions.Logging; // Cho logging (tùy chọn)
using System.Linq; // Nếu cần LINQ thêm


namespace AntTech.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Account> _userManager;     // Thay 'Account' bằng lớp User Identity của bạn
        private readonly SignInManager<Account> _signInManager; // Thay 'Account' bằng lớp User Identity của bạn
        private readonly ILogger<AccountController> _logger;    // Tùy chọn: Để ghi log

        public AccountController(
            UserManager<Account> userManager,
            SignInManager<Account> signInManager,
            ILogger<AccountController> logger) // Thêm ILogger nếu muốn
        {
            _userManager = userManager ?? throw new ArgumentNullException(nameof(userManager));
            _signInManager = signInManager ?? throw new ArgumentNullException(nameof(signInManager));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        // GET: /Account/Login
        [HttpGet]
        [AllowAnonymous] // Cho phép truy cập ngay cả khi chưa đăng nhập
        public IActionResult Login(string returnUrl = null)
        {
            // Nếu người dùng đã đăng nhập, chuyển hướng họ đi
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }

            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken] // Rất quan trọng
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null) // returnUrl có thể lấy từ form hoặc query string
        {
            // Đảm bảo returnUrl được truyền đi đúng cách
            model.ReturnUrl = model.ReturnUrl ?? returnUrl ?? Url.Content("~/"); // Nếu model.ReturnUrl null, thử lấy từ parameter, nếu vẫn null thì về trang chủ
            ViewData["ReturnUrl"] = model.ReturnUrl;

            if (ModelState.IsValid)
            {
                // Cố gắng tìm user bằng Email hoặc Username
                // Giả sử lớp Account của bạn có thuộc tính Email (thường là chuẩn của IdentityUser)
                // Nếu không, bạn cần điều chỉnh logic tìm kiếm này
                var user = await _userManager.FindByEmailAsync(model.EmailOrUsername)
                           ?? await _userManager.FindByNameAsync(model.EmailOrUsername);

                if (user != null)
                {
                    // Thử đăng nhập bằng mật khẩu
                    // PasswordSignInAsync sẽ tự động băm mật khẩu người dùng nhập và so sánh
                    var result = await _signInManager.PasswordSignInAsync(
                        user,               // User tìm được (hoặc user.UserName nếu FindByNameAsync là chính)
                        model.Password,
                        model.RememberMe,   // Lưu cookie lâu dài nếu true
                        lockoutOnFailure: false // Đặt true nếu muốn kích hoạt khóa tài khoản sau nhiều lần sai
                    );

                    if (result.Succeeded)
                    {
                        _logger.LogInformation($"User '{user.Username}' logged in successfully.");
                        return RedirectToLocal(model.ReturnUrl); // Chuyển hướng đến trang yêu cầu hoặc trang chủ
                    }
                    if (result.RequiresTwoFactor) // Nếu bạn có cấu hình xác thực 2 yếu tố
                    {
                        // return RedirectToAction(nameof(LoginWith2fa), new { returnUrl = model.ReturnUrl, rememberMe = model.RememberMe });
                        ModelState.AddModelError(string.Empty, "Yêu cầu xác thực hai yếu tố.");
                        return View(model);
                    }
                    if (result.IsLockedOut) // Nếu tài khoản bị khóa
                    {
                        _logger.LogWarning($"User account '{user.Username}' locked out.");
                        // return View("Lockout"); // Trả về View Lockout
                        ModelState.AddModelError(string.Empty, "Tài khoản đã bị khóa. Vui lòng thử lại sau.");
                        return View(model);
                    }
                    else // Các trường hợp đăng nhập thất bại khác (sai mật khẩu)
                    {
                        ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác.");
                        _logger.LogWarning($"Invalid login attempt for user '{model.EmailOrUsername}'.");
                        return View(model);
                    }
                }
                else // Không tìm thấy user với Email/Username đã nhập
                {
                    ModelState.AddModelError(string.Empty, "Thông tin đăng nhập không chính xác.");
                    _logger.LogWarning($"User with email/username '{model.EmailOrUsername}' not found.");
                    return View(model);
                }
            }

            // Nếu ModelState không hợp lệ (ví dụ: người dùng không nhập đủ thông tin)
            // Quay lại form đăng nhập với các lỗi validation
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout(string returnUrl = null)
        {
            await _signInManager.SignOutAsync(); // Đăng xuất người dùng hiện tại
            _logger.LogInformation("User logged out.");

            // Chuyển hướng về trang chủ hoặc returnUrl nếu có và hợp lệ
            return RedirectToLocal(returnUrl);
        }

        // GET: /Account/Register (Placeholder - Bạn cần tạo ViewModel và View riêng)
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register(string returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            // return View(new RegisterViewModel()); // Cần RegisterViewModel
            return RedirectToAction(nameof(Login), new { returnUrl }); // Tạm thời chuyển về Login
        }

        // Hàm tiện ích để chuyển hướng an toàn, tránh Open Redirect attacks
        private IActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl)) // Kiểm tra returnUrl có phải là local không
            {
                return Redirect(returnUrl);
            }
            else
            {
                // Mặc định chuyển hướng về trang chủ nếu returnUrl không hợp lệ hoặc trống
                return RedirectToAction(nameof(HomeController.Index), "Home");
            }
        }

        // Action cho trang Access Denied (nếu cần)
        // [HttpGet]
        // public IActionResult AccessDenied()
        // {
        //     return View();
        // }
    }
}