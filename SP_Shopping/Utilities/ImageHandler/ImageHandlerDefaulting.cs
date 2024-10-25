
using SP_Shopping.Utilities.ImageHandlerKeys;

namespace SP_Shopping.Utilities.ImageHandler;

public class ImageHandlerDefaulting<TKey>(string folderPath, string defaultProp, string keyName, string imgExtension = "png") : ImageHandlerDefaultingBase<TKey>(folderPath) where TKey : IImageHandlerKey
{
    protected override string DefaultProp => defaultProp;

    protected override string KeyName => keyName;

    protected override string ImgExtension => imgExtension;

}
