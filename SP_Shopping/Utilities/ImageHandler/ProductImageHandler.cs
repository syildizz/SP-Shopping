using SP_Shopping.Models;

namespace SP_Shopping.Utilities.ImageHandler;

public class ProductImageHandler(string folderPath) : ImageHandlerDefaultingBase<Product>(folderPath)
{
    protected override string DefaultProp => "default_product";

    protected override string KeyName => "product";

    protected override string Identifier(Product key) => $"{key.Id}_product";

}
