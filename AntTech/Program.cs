// <<<<<<< HEAD
// using Microsoft.Extensions.FileProviders; // Dòng này có thể không cần nếu không dùng ở đâu khác
// =======
using Microsoft.EntityFrameworkCore;
using AntTech.Models; // Đảm bảo namespace này chứa BlogWebsiteDbContext và lớp Account (IdentityUser)
using Microsoft.AspNetCore.Identity; // <<< THÊM USING NÀY CHO IDENTITY
using Microsoft.Extensions.FileProviders; // <<< THÊM USING NÀY CHO PhysicalFileProvider (nếu chưa có ở trên)
using System.IO; // <<< THÊM USING NÀY CHO Path.Combine và Directory (nếu chưa có)
// >>>>>>> d4872eee44a8e326a15f39245f47ef1727b85df7

var builder = WebApplication.CreateBuilder(args);

// --- Lấy chuỗi kết nối từ appsettings.json ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
}

// --- Đăng ký DbContext ---
builder.Services.AddDbContext<BlogWebsiteDbContext>(options => // !!! THAY BlogWebsiteDbContext BẰNG TÊN DbContext CỦA BẠN !!!
    options.UseSqlServer(connectionString));

// --- (Tùy chọn) Cấu hình đường dẫn cho trang đăng nhập, đăng xuất, access denied ---
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.ExpireTimeSpan = TimeSpan.FromMinutes(60);
    options.LoginPath = "/Account/Login";       // Đường dẫn đến trang đăng nhập
    options.LogoutPath = "/Account/Logout";     // Đường dẫn đến action đăng xuất
    options.AccessDeniedPath = "/Account/AccessDenied"; // Trang Access Denied
    options.SlidingExpiration = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}
else // Thêm else để có thể dùng DeveloperExceptionPage cho môi trường Development
{
    app.UseDeveloperExceptionPage(); // Hoặc để trống nếu .NET 6+ tự xử lý tốt     // Hữu ích nếu dùng EF Migrations và Identity
}


app.UseHttpsRedirection();
app.UseStaticFiles(); // Cho phép phục vụ file từ wwwroot

app.UseRouting();

// ===>>> THÊM UseAuthentication() TRƯỚC UseAuthorization() <<<===
app.UseAuthentication(); // QUAN TRỌNG: Kích hoạt xác thực
app.UseAuthorization();  // Kích hoạt phân quyền

// Cấu hình phục vụ file tĩnh từ thư mục uploads
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(builder.Environment.ContentRootPath, "wwwroot/uploads")), // Sửa: dùng builder.Environment.ContentRootPath
    RequestPath = "/uploads"
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Nếu bạn scaffolded Identity UI, nó có thể thêm dòng này:
// app.MapRazorPages();

app.Run();