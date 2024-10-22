using Microsoft.AspNetCore.Identity;

namespace SP_Shopping.Utilities;

public class UserImageHandler : IUserImageHandler
{
    private readonly string _profilePictureIdentifier = "pfp";
    private readonly string _imgExtension = ".png";
    private readonly string _folderPath;

    public UserImageHandler(string folderPath)
    {
        _folderPath = folderPath;
    }

    private string GenerateProfilePictureFileName(IdentityUser user)
    {
        return $"{user.Id}_{_profilePictureIdentifier}{_imgExtension}";
    }

    private string GenerateProfilePicturePath(IdentityUser user)
    {
        return Path.Combine(_folderPath, "img-content", GenerateProfilePictureFileName(new IdentityUser() { Id = user.Id }));
    }

    private string GenerateDefaultProfilePicturePath()
    {
        return Path.Combine(_folderPath, "img", GenerateProfilePictureFileName(new IdentityUser() { Id = "default" }));
    }

    public string GenerateProfilePictureURL(IdentityUser user)
    {
        return $"/img-content/{GenerateProfilePictureFileName(user)}";
    }

    public string GenerateDefaultProfilePictureURL()
    {
        return $"/img/{GenerateProfilePictureFileName(new IdentityUser() { Id = "default" })}";
    }

    public string GetProfilePictureOrDefaultURL(IdentityUser user)
    {
        return ProfilePictureExists(user) ? GenerateProfilePictureURL(user) : GenerateDefaultProfilePictureURL();
    }

    public bool ProfilePictureExists(IdentityUser user)
    {
        return File.Exists(GenerateProfilePicturePath(user));
    }

    public byte[] GetProfilePicture(IdentityUser user)
    {
        byte[] image = File.ReadAllBytes(GenerateProfilePicturePath(user));
        return image;
    }

    public async Task<byte[]> GetProfilePictureAsync(IdentityUser user)
    {
        byte[] image = await File.ReadAllBytesAsync(GenerateProfilePicturePath(user));
        return image;
    }

    public Stream GetProfilePictureStream(IdentityUser user)
    {
        return new FileStream(GenerateProfilePicturePath(user), FileMode.OpenOrCreate, FileAccess.Read);
    }

    public byte[] GetDefaultProfilePicture()
    {
        byte[] image = File.ReadAllBytes(GenerateDefaultProfilePicturePath());
        return image;
    }

    public Stream GetDefaultProfilePictureStream()
    {
        return new FileStream(GenerateDefaultProfilePicturePath(), FileMode.Open, FileAccess.Read);
    }

    private void ProcessImageData(Image image)
    {
        image.Mutate(o => o
            .Resize(480, 480)
        );
    }

    public bool SetProfilePicture(IdentityUser user, byte[] imageData)
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

    public bool SetProfilePicture(IdentityUser user, Stream stream)
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

    public async Task<bool> SetProfilePictureAsync(IdentityUser user, byte[] imageData)
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

    public async Task<bool> SetProfilePictureAsync(IdentityUser user, Stream stream)
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

    public void DeleteProfilePicture(IdentityUser user)
    {
        File.Delete(GenerateProfilePicturePath(user));
    }

}
