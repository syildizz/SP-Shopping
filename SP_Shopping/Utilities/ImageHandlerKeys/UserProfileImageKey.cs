namespace SP_Shopping.Utilities.ImageHandlerKeys;

public class UserProfileImageKey(string id) : IImageHandlerKey
{

    private readonly string id = id;

    public string Identifier()
    {
        return $"{id}_pfp";
    }
}
