
namespace SP_Shopping.Utilities.ImageHandlerKeys;

public class ProductImageKey(int id) : IImageHandlerKey
{

    public readonly int id = id;

    public string Identifier()
    {
        return $"{id}_product";
    }




}
