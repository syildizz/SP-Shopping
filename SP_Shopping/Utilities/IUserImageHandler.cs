using Microsoft.AspNetCore.Identity;

namespace SP_Shopping.Utilities;
public interface IUserImageHandler
{
    string GenerateDefaultProfilePictureURL();
    string GenerateProfilePictureURL(IdentityUser user);
    byte[] GetDefaultProfilePicture();
    byte[] GetProfilePicture(IdentityUser user);
    Task<byte[]> GetProfilePictureAsync(IdentityUser user);
    bool ProfilePictureExists(IdentityUser user);
    void SetProfilePicture(IdentityUser user, byte[] image);
    Task SetProfilePictureAsync(IdentityUser user, byte[] image);
}