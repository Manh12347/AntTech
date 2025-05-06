using System.ComponentModel.DataAnnotations;

namespace AntTech.Models
{
    public class Tag
    {
        [Key]
        public int TagId { get; set; }

        [StringLength(30)]
        public string TagName { get; set; }

        // --- Thuộc tính điều hướng ---
        // Quan hệ Nhiều-Nhiều với Article (qua TagInArticle)
        public virtual ICollection<TagInArticle> ArticleTags { get; set; } = new HashSet<TagInArticle>();

        // Quan hệ Nhiều-Nhiều với Account (qua PreferTag)
        public virtual ICollection<PreferTag> PreferringAccounts { get; set; } = new HashSet<PreferTag>();
    }
}
