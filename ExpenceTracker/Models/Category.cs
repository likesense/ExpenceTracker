using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenceTracker.Models
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }
        [Column(TypeName = "Nvarchar(50)")]
        [Required(ErrorMessage = "Требуется название.")]
        public string Title { get; set; }
        [Column(TypeName = "Nvarchar(5)")]
        [Required(ErrorMessage = "Требуется иконка (win + .)")]
        public string Icon { get; set; } = "";
        [Column(TypeName = "Nvarchar(10)")]
        public string Type { get; set; } = "Расход";
        // объединение значка и заголовка
        [NotMapped]
        public string? TitleWithIcon { 
            get
            {
                return this.Icon + " " + this.Title;
            }
        }   

    }
}
