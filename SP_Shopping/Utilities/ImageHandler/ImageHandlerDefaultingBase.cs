
namespace SP_Shopping.Utilities.ImageHandler;

public abstract class ImageHandlerDefaultingBase<TKey>(string folderPath) : ImageHandlerBase<TKey>(folderPath), IImageHandlerDefaulting<TKey>
{
    protected abstract string DefaultProp { get; }

    protected string GenerateDefaultImageFileName()
    {
        return $"{DefaultProp}.{ImgExtension}";
    }

    protected string GenerateDefaultProfilePicturePath()
    {
        return Path.Combine(FolderPath, "img", GenerateDefaultImageFileName());
    }

    public string GenerateDefaultImageURL()
    {
        return Path.Combine("/", "img", $"{GenerateDefaultImageFileName()}");
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
