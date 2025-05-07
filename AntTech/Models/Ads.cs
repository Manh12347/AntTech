using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AntTech.Models
{
    [Table("Ads")]
    public class Ads
    {
        [Key]
        public int AdsId { get; set; }

        public string Link { get; set; } // varchar(Max) -> string

        [Column(TypeName = "decimal(10, 2)")] // Chỉ định rõ kiểu decimal trong DB
        public decimal? XSize { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? YSize { get; set; }

        public int ClickCount { get; set; } = 0;

        // Thêm trường mới
        public string ImageUrl { get; set; }

        public string AdType { get; set; } = "sidebar"; // sidebar, banner

        public bool IsActive { get; set; } = true;

        public DateTime StartDate { get; set; } = DateTime.Now;

        public DateTime? EndDate { get; set; }

        [NotMapped]
        public bool IsVisible => IsActive &&
                                (EndDate == null || EndDate >= DateTime.Now) &&
                                StartDate <= DateTime.Now;
    }
}
