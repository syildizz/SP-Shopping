using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos.Cart;

public class AdminCartItemDetailsDto
{
    [DisplayName("Product Id")]
    public int ProductId { get; set; }
    [DisplayName("Product Name")]
    public string ProductName { get; set; }
    [DisplayName("Cart Owner Id")]
    public string UserId { get; set; }
    [DisplayName("Cart Owner Name")]
    public string UserName { get; set; }
    [DisplayName("Seller Id")]
    public string SubmitterId { get; set; }
    [DisplayName("Seller Name")]
    public string SubmitterName { get; set; }
    [RegularExpression("([0-9]+)", ErrorMessage = "Please enter valid Number")]
    [Range(0, int.MaxValue)]
    public int Count { get; set; }
}
