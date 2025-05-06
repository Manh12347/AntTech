using Microsoft.EntityFrameworkCore;

namespace AntTech.Models
{
    public class BlogWebsiteDbContext : DbContext
    {
        public BlogWebsiteDbContext(DbContextOptions<BlogWebsiteDbContext> options) : base(options) { }

        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountInfo> AccountInfos { get; set; }
        public DbSet<FollowerList> FollowerLists { get; set; }
        public DbSet<SecurityLog> SecurityLogs { get; set; }
        public DbSet<Notify> Notifies { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<PhotoInArticle> PhotoInArticles { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<TagInArticle> TagInArticles { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Ads> Ads { get; set; }
        public DbSet<PreferTag> PreferTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình khóa chính phức hợp bằng Fluent API
            modelBuilder.Entity<FollowerList>()
                .HasKey(fl => new { fl.AccountId, fl.Follower }); // Đảm bảo tên cột đúng

            modelBuilder.Entity<Author>()
                .HasKey(a => new { a.AccountId, a.ArticleId });

            modelBuilder.Entity<TagInArticle>()
                .HasKey(tia => new { tia.ArticleId, tia.TagId });

            modelBuilder.Entity<PreferTag>()
                .HasKey(pt => new { pt.AccountId, pt.TagId });

            // Cấu hình quan hệ cho FollowerList nếu cần rõ ràng hơn (tùy chọn)
            modelBuilder.Entity<FollowerList>()
                .HasOne(fl => fl.FollowedAccount)
                .WithMany(a => a.Followers)
                .HasForeignKey(fl => fl.AccountId)
                .OnDelete(DeleteBehavior.Restrict); // Hoặc Cascade, SetNull tùy logic

            modelBuilder.Entity<FollowerList>()
                .HasOne(fl => fl.FollowerAccount)
                .WithMany(a => a.Following)
                .HasForeignKey(fl => fl.Follower) // Đảm bảo tên cột đúng
                .OnDelete(DeleteBehavior.Restrict); // Hoặc Cascade, SetNull tùy logic


            // Thêm các cấu hình Fluent API khác nếu cần (ví dụ: Indexes, Constraints, ...)
        }
    }
}
