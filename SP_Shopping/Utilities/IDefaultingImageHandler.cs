namespace SP_Shopping.Utilities;

public interface IDefaultingImageHandler<TKey> : IImageHandler<TKey>
{
    string GenerateDefaultImageURL();
    string GetImageOrDefaultURL(TKey key);
    byte[] GetDefaultImageData();
    Stream GetDefaultImageStream();
}
