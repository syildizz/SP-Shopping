using Microsoft.AspNetCore.Identity;
using SP_Shopping.Models;

namespace SP_Shopping.Utilities.ImageHandler;

public class UserProfileImageHandler(string folderPath) : ImageHandlerDefaultingBase<IdentityUser>(folderPath)
{
    protected override string DefaultProp => "default_pfp";

    protected override string KeyName => "user-pfp";

    protected override string ImgExtension => "png";

    protected override string Identifier(IdentityUser key) => $"{key.Id}_pfp";
}
