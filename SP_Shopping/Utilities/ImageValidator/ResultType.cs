namespace SP_Shopping.Utilities.ImageValidator;

public enum ResultType
{
    Success,
    ContentTypeIsNotImage,
    LengthIsNotWithinLimits,
    InvalidImageFormat,
    InvalidImageContent
}
