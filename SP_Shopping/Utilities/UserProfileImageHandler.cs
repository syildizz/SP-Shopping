using Microsoft.AspNetCore.Identity;

namespace SP_Shopping.Utilities;

public class UserProfileImageHandler : IDefaultingImageHandler<IdentityUser>
{
    private readonly string _profilePictureIdentifier = "pfp";
    private readonly string _imgExtension = ".png";
    private readonly string _folderPath;

    public UserProfileImageHandler(string folderPath)
    {
        _folderPath = folderPath;
    }

    private string GenerateProfilePictureFileName(IdentityUser user)
    {
        return $"{user.Id}_{_profilePictureIdentifier}{_imgExtension}";
    }

    private string GenerateProfilePicturePath(IdentityUser user)
    {
        return Path.Combine(_folderPath, "img-content", "user", GenerateProfilePictureFileName(new IdentityUser() { Id = user.Id }));
    }

    private string GenerateDefaultProfilePicturePath()
    {
        return Path.Combine(_folderPath, "img", GenerateProfilePictureFileName(new IdentityUser() { Id = "default" }));
    }

    public string GenerateImageURL(IdentityUser user)
    {
        return Path.Combine("/", "img-content", "user", $"{GenerateProfilePictureFileName(user)}");
    }

    public string GenerateDefaultImageURL()
    {
        return Path.Combine("/", "img", $"{GenerateProfilePictureFileName(new IdentityUser() { Id = "default" })}");
    }

    public string GetImageOrDefaultURL(IdentityUser user)
    {
        return ImageExists(user) ? GenerateImageURL(user) : GenerateDefaultImageURL();
    }

    public byte[] GetImageData(IdentityUser user)
    {
        byte[] image = File.ReadAllBytes(GenerateProfilePicturePath(user));
        return image;
    }

    public byte[] GetDefaultImageData()
    {
        byte[] image = File.ReadAllBytes(GenerateDefaultProfilePicturePath());
        return image;
    }

    public async Task<byte[]> GetImageDataAsync(IdentityUser user)
    {
        byte[] image = await File.ReadAllBytesAsync(GenerateProfilePicturePath(user));
        return image;
    }

    public Stream GetImageStream(IdentityUser user)
    {
        return new FileStream(GenerateProfilePicturePath(user), FileMode.OpenOrCreate, FileAccess.Read);
    }

    public Stream GetDefaultImageStream()
    {
        return new FileStream(GenerateDefaultProfilePicturePath(), FileMode.Open, FileAccess.Read);
    }

    public bool ImageExists(IdentityUser user)
    {
        return File.Exists(GenerateProfilePicturePath(user));
    }

    private void ProcessImageData(Image image)
    {
        image.Mutate(o => o
            .Resize(480, 480)
        );
    }

    public bool SetImage(IdentityUser user, byte[] imageData)
    {
        try
        {
            using Image image = Image.Load(imageData);
            ProcessImageData(image);
            image.SaveAsPng(GenerateProfilePicturePath(user));
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

    public bool SetImage(IdentityUser user, Stream stream)
    {
        try
        {
            Image image = Image.Load(stream);
            ProcessImageData(image);
            image.SaveAsPng(GenerateProfilePicturePath(user));
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

    public async Task<bool> SetImageAsync(IdentityUser user, byte[] imageData)
    {
        try
        {
            using Image image = Image.Load(imageData);
            ProcessImageData(image);
            await image.SaveAsPngAsync(GenerateProfilePicturePath(user));
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

    public async Task<bool> SetImageAsync(IdentityUser user, Stream stream)
    {
        try
        {
            using Image image = await Image.LoadAsync(stream);
            ProcessImageData(image);
            await image.SaveAsPngAsync(GenerateProfilePicturePath(user));
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

    public void DeleteImage(IdentityUser user)
    {
        File.Delete(GenerateProfilePicturePath(user));
    }

}
