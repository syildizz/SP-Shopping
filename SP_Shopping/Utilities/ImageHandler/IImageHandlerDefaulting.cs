namespace SP_Shopping.Utilities.ImageHandler;

public interface IImageHandlerDefaulting<TKey> : IImageHandler<TKey>
{
    string GenerateDefaultImageURL();
    string GetImageOrDefaultURL(TKey key);
    byte[] GetDefaultImageData();
    Stream GetDefaultImageStream();
}
