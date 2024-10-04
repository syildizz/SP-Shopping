using SP_Shopping.Models;
using System.Globalization;
using System.Text.Json;

namespace SP_Shopping.Data;

public class ProductDataFromJson()
{
    public string Name { get; set; }
    public Category Category { get; set; }
    public string Price {  get; set; }
}

public class ProductSeederFromJson(string fileName)
{
    private readonly string _fileName = fileName;
    
    public IEnumerable<Product>? MakeProductList()
    {
        using var fileStream = File.OpenRead(_fileName);
        var stringResult = JsonSerializer.Deserialize<List<ProductDataFromJson>>(fileStream);
        IEnumerable<Product>? result = stringResult?.
            Select(p => new Product() 
            { 
                Name = p.Name, 
                Price = Convert.ToDecimal(p.Price, CultureInfo.InvariantCulture), 
                Category = p.Category 
            })
        ;
        return result;
    }

}
