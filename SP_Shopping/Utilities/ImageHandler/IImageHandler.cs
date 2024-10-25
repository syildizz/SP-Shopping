
using SP_Shopping.Utilities.ImageHandlerKeys;

namespace SP_Shopping.Utilities.ImageHandler;

public interface IImageHandler<TKey> where TKey : IImageHandlerKey
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
    public bool ValidateImage(byte[] imageData);
    public bool ValidateImage(Stream stream);
    public Task<bool> ValidateImageAsync(Stream stream);
    void DeleteImage(TKey key);
}
