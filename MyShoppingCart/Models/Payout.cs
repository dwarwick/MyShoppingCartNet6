using MyShoppingCart.Data.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyShoppingCart.Models
{
    public class Payout : IEntityBase
    {
        [Key]
        public int Id { get; set; }
        
        [Required]
        public ApplicationUser applicationUser { get; set; }
        [Range(0.0, 100000000.0)]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal payoutAmount { get; set; }
        public DateTime payoutDate { get; set; } = DateTime.UtcNow;
        public string BatchStatus { get; set; }
        public string BatchId { get; set; }
        public string EmailMessage { get; set; }
        public string EmailSubject { get; set; }

    }
}
