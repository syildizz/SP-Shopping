using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration.UserSecrets;
using SP_Shopping.Data.Migrations;

namespace SP_Shopping.Handlers;

public class UserImageHandler
{
    private readonly string _userId;
    private readonly string _profilePictureIndetifier = "pfp";
    private readonly string _imgExtension = ".png";
    private string ImagePath => $"{_userId}_{_profilePictureIndetifier}{_imgExtension}";
    public UserImageHandler(IdentityUser user)
    {
        _userId = user.Id;
    }

    public UserImageHandler(string userId)
    {
        _userId = userId;
    }

    public async Task<byte[]> GetProfilePicture()
    {
        byte[] image = await File.ReadAllBytesAsync(Path.Combine(Directory.GetCurrentDirectory(), ImagePath));
        return image;
    }

    public async Task SetProfilePicture(byte[] image)
    {
        await File.WriteAllBytesAsync(ImagePath, image);
    }

}
