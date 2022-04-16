using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayoutsJobConsoleApp.Models
{
    public class ProductImage 
    {
        [Key]
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ImageURL { get; set; }
        public string ImageName { get; set; }

        [Display(Name = "Image Description")]
        [StringLength(25, ErrorMessage = "Maximum length of description: 25 charcters")]
        public string ImageDescription { get; set; }

        [ForeignKey("ProductId")]
        public Product product { get; set; }

    }
}
