namespace SP_Shopping.Utilities;

public interface IImageHandler<TKey>
{
    string GenerateImageURL(TKey key);
    byte[] GetImageData(TKey key);
    Task<byte[]> GetImageDataAsync(TKey key);
    Stream GetImageStream(TKey key);
    bool ImageExists(TKey key);
    bool SetImage(TKey key, byte[] imageData);
    bool SetImage(TKey key, Stream stream);
    Task<bool> SetImageAsync(TKey key, byte[] imageData);
    Task<bool> SetImageAsync(TKey key, Stream stream);
    void DeleteImage(TKey key);
}
