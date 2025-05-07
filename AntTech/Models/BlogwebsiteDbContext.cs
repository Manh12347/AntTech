using Microsoft.EntityFrameworkCore;
// Đảm bảo các using đến namespace chứa các lớp Entity của bạn (thường là AntTech.Models)
using AntTech.Models;

namespace AntTech.Models // Hoặc AntTech.Data, hoặc namespace chung bạn chọn
{
    public class BlogWebsiteDbContext : DbContext
    {
        public BlogWebsiteDbContext(DbContextOptions<BlogWebsiteDbContext> options)
            : base(options)
        {
        }

        // --- Khai báo DbSets ---
        public DbSet<Account> Accounts { get; set; }
        public DbSet<AccountInfo> AccountInfos { get; set; }
        public DbSet<AccountEmail> AccountEmails { get; set; } // Mới thêm
        public DbSet<Article> Articles { get; set; }
        public DbSet<Author> Authors { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<CommentLike> CommentLikes { get; set; } // Mới thêm
        public DbSet<FollowerList> FollowerLists { get; set; }
        public DbSet<Notify> Notifies { get; set; }
        public DbSet<PhotoInArticle> PhotoInArticles { get; set; }
        public DbSet<PreferTag> PreferTags { get; set; }
        public DbSet<SecurityLog> SecurityLogs { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagInArticle> TagInArticles { get; set; }
        public DbSet<Ads> Ads { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // --- Cấu hình ánh xạ tên bảng (Table Mapping) ---
            // Đảm bảo tên trong ToTable("...") khớp với tên bảng thực tế trong SQL Server
            modelBuilder.Entity<Account>().ToTable("Account");
            modelBuilder.Entity<AccountInfo>().ToTable("AccountInfo");
            modelBuilder.Entity<AccountEmail>().ToTable("AccountEmail"); // Ánh xạ bảng AccountEmail
            modelBuilder.Entity<Article>().ToTable("Article");
            modelBuilder.Entity<Author>().ToTable("Author");
            modelBuilder.Entity<Comment>().ToTable("Comment");
            modelBuilder.Entity<CommentLike>().ToTable("CommentLike"); // Ánh xạ bảng CommentLike
            modelBuilder.Entity<FollowerList>().ToTable("FollowerList");
            modelBuilder.Entity<Notify>().ToTable("Notify");
            modelBuilder.Entity<PhotoInArticle>().ToTable("PhotoInArticle");
            modelBuilder.Entity<PreferTag>().ToTable("PreferTag");
            modelBuilder.Entity<SecurityLog>().ToTable("SecurityLog");
            modelBuilder.Entity<Tag>().ToTable("Tag");
            modelBuilder.Entity<TagInArticle>().ToTable("TagInArticle");
            modelBuilder.Entity<Ads>().ToTable("Ads");

            // --- Cấu hình Khóa Chính Phức Hợp (Composite Primary Keys) ---
            modelBuilder.Entity<FollowerList>()
                .HasKey(fl => new { fl.AccountId, fl.Follower });

            modelBuilder.Entity<Author>()
                .HasKey(a => new { a.AccountId, a.ArticleId });

            modelBuilder.Entity<TagInArticle>()
                .HasKey(tia => new { tia.ArticleId, tia.TagId });

            modelBuilder.Entity<PreferTag>()
                .HasKey(pt => new { pt.AccountId, pt.TagId });

            modelBuilder.Entity<CommentLike>() // Khóa chính cho CommentLike
                .HasKey(cl => new { cl.CommentId, cl.AccountId });


            // --- Cấu hình Mối Quan Hệ Chi Tiết (Relationships) ---

            // 1. Quan hệ 1-1 giữa Account và AccountInfo (thường tự động phát hiện)
            // modelBuilder.Entity<Account>()
            //     .HasOne(a => a.AccountInfo)
            //     .WithOne(ai => ai.Account)
            //     .HasForeignKey<AccountInfo>(ai => ai.AccountId);

            // 2. Quan hệ 1-1 giữa Account và AccountEmail
            modelBuilder.Entity<Account>()
                .HasOne(a => a.AccountEmail)        // Account có một AccountEmail (Navigation Property trong Account)
                .WithOne(ae => ae.Account)          // AccountEmail thuộc về một Account (Navigation Property trong AccountEmail)
                .HasForeignKey<AccountEmail>(ae => ae.AccountId); // Khóa ngoại là AccountEmail.AccountId

            // 3. Quan hệ Reply cho Comment (tự tham chiếu 1-Nhiều)
            modelBuilder.Entity<Comment>()
                .HasMany(c => c.Replies)                // Một Comment gốc có nhiều Replies
                .WithOne(r => r.ParentComment)          // Mỗi Reply trỏ đến một ParentComment
                .HasForeignKey(r => r.ParentCommentId)  // Khóa ngoại là ParentCommentId
                .OnDelete(DeleteBehavior.Restrict);     // Hoặc ClientSetNull. Không nên Cascade ở đây để tránh lỗi vòng lặp.
                                                        // Restrict: Ngăn xóa comment cha nếu còn reply.
                                                        // ClientSetNull: Khi comment cha bị xóa, ParentCommentId của các reply con sẽ được đặt thành NULL.

            // 4. Quan hệ cho CommentLike (Nhiều-Nhiều giữa Comment và Account thông qua CommentLike)
            modelBuilder.Entity<CommentLike>()
                .HasOne(cl => cl.Comment)               // CommentLike trỏ đến một Comment
                .WithMany(c => c.CommentLikes)          // Comment có nhiều CommentLikes (ICollection<CommentLike> trong Comment)
                .HasForeignKey(cl => cl.CommentId)
                .OnDelete(DeleteBehavior.Cascade);      // Khi Comment bị xóa, các CommentLikes cũng bị xóa

            modelBuilder.Entity<CommentLike>()
                .HasOne(cl => cl.Account)               // CommentLike trỏ đến một Account
                .WithMany()                             // Giả định Account không có Navigation Property ngược lại là ICollection<CommentLike>
                                                        // Nếu Account có (ví dụ: ICollection<CommentLike> LikedCommentsByThisUser), thì dùng:
                                                        // .WithMany(a => a.LikedCommentsByThisUser)
                .HasForeignKey(cl => cl.AccountId)
                .OnDelete(DeleteBehavior.Cascade);      // Khi Account bị xóa, CommentLikes của Account đó cũng bị xóa
                                                        // (Cân nhắc DeleteBehavior.Restrict nếu không muốn điều này)

            // 5. Quan hệ FollowerList (Nhiều-Nhiều tự tham chiếu Account)
            modelBuilder.Entity<FollowerList>()
                .HasOne(fl => fl.FollowedAccount)               // Entity chính cho AccountId (Người được theo dõi)
                .WithMany(a => a.Followers)             // Account có nhiều Followers (ICollection<FollowerList> trong Account)
                .HasForeignKey(fl => fl.AccountId)      // Khóa ngoại là AccountId
                .OnDelete(DeleteBehavior.Restrict);     // Không cho xóa Account nếu vẫn còn người theo dõi Account đó

            modelBuilder.Entity<FollowerList>()
                .HasOne(fl => fl.FollowerAccount)       // Entity chính cho Follower (Người đi theo dõi)
                .WithMany(a => a.Following)             // Account đang theo dõi nhiều người (ICollection<FollowerList> trong Account)
                .HasForeignKey(fl => fl.Follower)       // Khóa ngoại là Follower
                .OnDelete(DeleteBehavior.Restrict);     // Không cho xóa Account nếu Account đó đang theo dõi người khác

            // --- Các cấu hình khác (nếu cần) ---
            // Ví dụ: Định nghĩa UNIQUE constraints, precision cho decimal, giá trị mặc định (nếu không đặt trong SQL)
            // modelBuilder.Entity<AccountInfo>()
            //    .HasIndex(ai => ai.UserId)
            //    .IsUnique();

            // modelBuilder.Entity<Account>()
            //    .HasIndex(a => a.Username)
            //    .IsUnique();

            // modelBuilder.Entity<Ads>()
            //     .Property(a => a.xSize)
            //     .HasPrecision(10, 2); // Nếu DB không tự nhận đúng
            // modelBuilder.Entity<Ads>()
            //     .Property(a => a.ySize)
            //     .HasPrecision(10, 2);
        }
    }
}