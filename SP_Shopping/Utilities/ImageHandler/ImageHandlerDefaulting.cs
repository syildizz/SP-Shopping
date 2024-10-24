namespace SP_Shopping.Utilities.ImageHandler;

public class ImageHandlerDefaulting<TKey>(string folderPath, string defaultProp, string keyName, Func<TKey, string> identifier, string imgExtension = "png") : ImageHandlerDefaultingBase<TKey>(folderPath)
{
    protected override string DefaultProp => defaultProp;

    protected override string KeyName => keyName;

    protected override string ImgExtension => imgExtension;

    protected override string Identifier(TKey key) => identifier(key);
}
