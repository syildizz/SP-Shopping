
namespace SP_Shopping.Utilities.ImageHandler;

public abstract class ImageHandlerBase<TKey>(string folderPath) : IImageHandler<TKey>
{
    protected abstract string KeyName { get; }
    protected abstract string Identifier(TKey key);
    protected string FolderPath => folderPath;
    protected virtual string ImgExtension => "png";

    protected string GenerateImageFileName(TKey key)
    {
        return $"{Identifier(key)}.{ImgExtension}";
    }

    protected string GenerateImagePath(TKey key)
    {
        return Path.Combine(FolderPath, "img-content", KeyName, GenerateImageFileName(key));
    }

    public string GenerateImageURL(TKey key)
    {
        return Path.Combine("/", "img-content", KeyName, $"{GenerateImageFileName(key)}");
    }

    public byte[] GetImageData(TKey key)
    {
        byte[] image = File.ReadAllBytes(GenerateImagePath(key));
        return image;
    }

    public async Task<byte[]> GetImageDataAsync(TKey key)
    {
        byte[] image = await File.ReadAllBytesAsync(GenerateImagePath(key));
        return image;
    }

    public Stream GetImageStream(TKey key)
    {
        return new FileStream(GenerateImagePath(key), FileMode.OpenOrCreate, FileAccess.Read);
    }

    public bool ImageExists(TKey key)
    {
        return File.Exists(GenerateImagePath(key));
    }

    protected virtual void ProcessImageData(Image image)
    {
        image.Mutate(o => o
            .Resize(480, 480)
        );
    }

    public bool SetImage(TKey key, byte[] imageData)
    {
        try
        {
            using Image image = Image.Load(imageData);
            ProcessImageData(image);
            image.Save(GenerateImagePath(key));
            return true;
        }
        catch (Exception ex)
        {
            if (ex is UnknownImageFormatException or OverflowException)
            {
                return false;
            }
            throw;
        }
    }

    public bool SetImage(TKey key, Stream stream)
    {
        try
        {
            Image image = Image.Load(stream);
            ProcessImageData(image);
            image.Save(GenerateImagePath(key));
            return true;
        }
        catch (Exception ex)
        {
            if (ex is UnknownImageFormatException or OverflowException)
            {
                return false;
            }
            throw;
        }
    }

    public async Task<bool> SetImageAsync(TKey key, byte[] imageData)
    {
        try
        {
            using Image image = Image.Load(imageData);
            ProcessImageData(image);
            await image.SaveAsync(GenerateImagePath(key));
            return true;
        }
        catch (Exception ex)
        {
            if (ex is UnknownImageFormatException or OverflowException)
            {
                return false;
            }
            throw;
        }
    }

    public async Task<bool> SetImageAsync(TKey key, Stream stream)
    {
        try
        {
            using Image image = await Image.LoadAsync(stream);
            ProcessImageData(image);
            await image.SaveAsync(GenerateImagePath(key));
            return true;
        }
        catch (Exception ex)
        {
            if (ex is UnknownImageFormatException or OverflowException)
            {
                return false;
            }
            throw;
        }
    }

    public void DeleteImage(TKey key)
    {
        var imagePath = GenerateImagePath(key);
        try
        {
            File.Delete(imagePath);
        }
        catch(DirectoryNotFoundException)
        {
            if (Directory.GetParent(imagePath)?.FullName is string parentDirectory) {
                Directory.CreateDirectory(parentDirectory);
            } 
            else
            {
                throw;
            }
        }
    }

}
