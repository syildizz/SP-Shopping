using System.ComponentModel.DataAnnotations.Schema;

namespace SP_Shopping.Models
{
    public class Order
    {
        public int Id { get; set; }
        [ForeignKey(nameof(User))]
        public required string UserId { get; set; }
        public required ApplicationUser User { get; set; }
        public List<Product> Products { get; set; } = default!;
        public DateTime InsertionDate { get; set; } 
        public DateTime? ModificationDate { get; set; }
    }
}
