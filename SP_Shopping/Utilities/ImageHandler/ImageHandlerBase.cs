
using SP_Shopping.Utilities.ImageHandlerKeys;

namespace SP_Shopping.Utilities.ImageHandler;

public abstract class ImageHandlerBase<TKey>(string folderPath) : IImageHandler<TKey> where TKey : IImageHandlerKey
{
    protected abstract string ImageFolder { get; }
    protected string FolderPath => folderPath;
    protected virtual string ImgExtension => "png";

    protected string GenerateImageFileName(TKey key)
    {
        return $"{key.Identifier()}.{ImgExtension}";
    }

    protected string GenerateImagePath(TKey key)
    {
        return Path.Combine(FolderPath, ImageFolder, GenerateImageFileName(key));
    }

    public string GenerateImageURL(TKey key)
    {
        return Path.Combine("/", ImageFolder, $"{GenerateImageFileName(key)}");
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
        image.Metadata.ExifProfile = null;
        image.Metadata.IccProfile = null;
        image.Metadata.IptcProfile = null;
        image.Metadata.XmpProfile = null;
        image.Metadata.CicpProfile = null;
        image.Mutate(o => o
            .Resize(480, 480)
            .GaussianBlur(0.01f)
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

    public bool ValidateImage(byte[] imageData)
    {
        try
        {
            using Image image = Image.Load(imageData);
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

    public bool ValidateImage(Stream stream)
    {
        try
        {
            using Image image = Image.Load(stream);
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

    public async Task<bool> ValidateImageAsync(Stream stream)
    {
        try
        {
            using Image image = await Image.LoadAsync(stream);
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
                DeleteImage(key);
            } 
            else
            {
                throw;
            }
        }
    }

}
