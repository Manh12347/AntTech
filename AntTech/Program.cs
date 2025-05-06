using Microsoft.EntityFrameworkCore; // <<< THÊM USING NÀY
using AntTech.Models; // <<< THÊM USING NÀY (Hoặc AntTech.Data nếu DbContext ở đó)

var builder = WebApplication.CreateBuilder(args);

// --- Lấy chuỗi kết nối từ appsettings.json ---
// Đảm bảo bạn có key "DefaultConnection" trong phần "ConnectionStrings" của appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString)) // Thêm kiểm tra nếu chuỗi kết nối rỗng
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.json.");
}

// ===>>> ĐĂNG KÝ DBCONTEXT Ở ĐÂY (TRƯỚC KHI BUILD) <<<===
builder.Services.AddDbContext<BlogWebsiteDbContext>(options =>
    options.UseSqlServer(connectionString)); // Cấu hình dùng SQL Server

// Add services to the container.
builder.Services.AddControllersWithViews(); // Dịch vụ này giữ nguyên

var app = builder.Build(); // ===>>> DÒNG BUILD() GIỮ NGUYÊN Ở ĐÂY <<<===

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();