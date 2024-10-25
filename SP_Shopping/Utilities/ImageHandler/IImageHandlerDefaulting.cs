
using SP_Shopping.Utilities.ImageHandlerKeys;

namespace SP_Shopping.Utilities.ImageHandler;

public interface IImageHandlerDefaulting<TKey> : IImageHandler<TKey> where TKey : IImageHandlerKey
{
    string GenerateDefaultImageURL();
    string GetImageOrDefaultURL(TKey key);
    byte[] GetDefaultImageData();
    Stream GetDefaultImageStream();
}
