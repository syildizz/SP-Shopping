using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Dtos;

public class OrderCreateDto
{
    [Required(ErrorMessage="You need to enter a username.")]
    public string UserName { get; set; }
    public List<string> ProductNames { get; set; }
}
