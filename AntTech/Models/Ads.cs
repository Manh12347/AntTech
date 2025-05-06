using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AntTech.Models
{
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
    }
}
