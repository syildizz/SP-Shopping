namespace SP_Shopping.Utilities.ImageValidator;

public enum ImageValidatorResultType
{
    Success,
    ContentTypeIsNotImage,
    LengthIsNotWithinLimits,
    InvalidImageFormat,
    InvalidImageContent
}
