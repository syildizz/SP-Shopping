
using SP_Shopping.Utilities.ImageHandlerKeys;

namespace SP_Shopping.Utilities.ImageHandler;

public class ImageHandlerDefaulting<TKey>
(
    string folderPath, 
    string defaultImageFolder, 
    string defaultProp, 
    string imageFolder, 
    string imgExtension = "png"
) : ImageHandlerDefaultingBase<TKey>(folderPath) where TKey : IImageHandlerKey
{
    protected override string DefaultProp => defaultProp;

    protected override string DefaultImageFolder => defaultImageFolder;

    protected override string ImageFolder => imageFolder;

    protected override string ImgExtension => imgExtension;

}
