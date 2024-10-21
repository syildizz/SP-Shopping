using Microsoft.AspNetCore.Identity;
using Microsoft.Identity.Client;

namespace SP_Shopping.Utilities;

public class UserImageHandler : IUserImageHandler
{
    private readonly string _profilePictureIdentifier = "pfp";
    private readonly string _imgExtension = ".png";
    private readonly string _folderPath;

    public UserImageHandler(IWebHostEnvironment henv)
    {
        _folderPath = henv.WebRootPath;
    }

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

    public byte[] GetDefaultProfilePicture()
    {
        byte[] image = File.ReadAllBytes(GenerateDefaultProfilePicturePath());
        return image;
    }

    public void SetProfilePicture(IdentityUser user, byte[] image)
    {
        File.WriteAllBytes(GenerateProfilePicturePath(user), image);
    }

    public async Task SetProfilePictureAsync(IdentityUser user, byte[] image)
    {
        await File.WriteAllBytesAsync(GenerateProfilePicturePath(user), image);
    }

    public void DeleteProfilePicture(IdentityUser user)
    {
        File.Delete(GenerateProfilePicturePath(user));
    }


}
