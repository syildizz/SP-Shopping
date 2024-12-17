
namespace SP_Shopping.Config;

//"ImageLocations": {
//  "Default": {
//    "Folder": "img",
//    "User": "default_pfp",
//    "Product": "default_product"
//  },
//  "User": {
//    "Folder": "img-content/user-pfp"
//  },
//  "Product": {
//    "Folder":  "img-content/product"
//  }
//}

public class ImageLocationsOption
{
    public const string ImageLocations = nameof(ImageLocations);

    public DefaultOption Default { get; set; }
    public UserOption User { get; set; }
    public ProductOption Product { get; set; }

    public class DefaultOption
    {
        public string Folder { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string Product { get; set; } = string.Empty;
    }

    public class UserOption
    {
        public string Folder { get; set; } = string.Empty;
    }

    public class ProductOption
    {
        public string Folder { get; set; } = string.Empty;
    }

}

