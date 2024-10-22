using Microsoft.AspNetCore.Identity;

namespace SP_Shopping.Utilities;
public interface IUserImageHandler
{
    void DeleteProfilePicture(IdentityUser user);
    string GenerateDefaultProfilePictureURL();
    string GenerateProfilePictureURL(IdentityUser user);
    byte[] GetDefaultProfilePicture();
    Stream GetDefaultProfilePictureStream();
    byte[] GetProfilePicture(IdentityUser user);
    Task<byte[]> GetProfilePictureAsync(IdentityUser user);
    string GetProfilePictureOrDefaultURL(IdentityUser user);
    Stream GetProfilePictureStream(IdentityUser user);
    bool ProfilePictureExists(IdentityUser user);
    bool SetProfilePicture(IdentityUser user, byte[] imageData);
    bool SetProfilePicture(IdentityUser user, Stream stream);
    Task<bool> SetProfilePictureAsync(IdentityUser user, byte[] imageData);
    Task<bool> SetProfilePictureAsync(IdentityUser user, Stream stream);
}