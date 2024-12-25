
using Microsoft.Identity.Client;
using SP_Shopping.Utilities.ImageHandlerKeys;

namespace SP_Shopping.Utilities.ImageHandler;

public abstract class ImageHandlerDefaultingBase<TKey>(string folderPath) : ImageHandlerBase<TKey>(folderPath), IImageHandlerDefaulting<TKey> where TKey : IImageHandlerKey
{
    protected abstract string DefaultProp { get; }
    protected abstract string DefaultImageFolder { get; }

    protected string GenerateDefaultImageFileName()
    {
        return $"{DefaultProp}.{ImgExtension}";
    }

    protected string GenerateDefaultProfilePicturePath()
    {
        return Path.Combine(FolderPath, DefaultImageFolder, GenerateDefaultImageFileName());
    }

    public string GenerateDefaultImageURL()
    {
        return $"/{DefaultImageFolder}/{GenerateDefaultImageFileName()}";
    }

    public string GetImageOrDefaultURL(TKey key)
    {
        return ImageExists(key) ? GenerateImageURL(key) : GenerateDefaultImageURL();
    }

    public byte[] GetDefaultImageData()
    {
        byte[] image = File.ReadAllBytes(GenerateDefaultProfilePicturePath());
        return image;
    }

    public Stream GetDefaultImageStream()
    {
        return new FileStream(GenerateDefaultProfilePicturePath(), FileMode.Open, FileAccess.Read);
    }

}
