
namespace SP_Shopping.Utilities.ImageValidator;

public class ImageValidator(long maxFileSizeByte = 1_500_000)
{
    public long MaxFileSizeByte { get; } = maxFileSizeByte;

    public Result Validate(IFormFile imageFile)
    {

        if (!imageFile.ContentType.Contains("image"))
        {
            return new Result
            (
                type: Result.ResultType.ContentTypeIsNotImage,
                defaultMessage: "File has to be an image."
            );
        }

        if (imageFile.Length > MaxFileSizeByte)
        {
            return new Result
            (
                type: Result.ResultType.LengthIsNotWithinLimits,
                defaultMessage: $"Cannot upload images larger than {MaxFileSizeByte / 1_000_000M}MB."
            );
        }

        try
        {
            using var stream = imageFile.OpenReadStream();
            Image.DetectFormat(stream);
        }
        catch (UnknownImageFormatException)
        {
            return new Result
            (
                type: Result.ResultType.InvalidImageFormat,
                defaultMessage: $"Image format is invalid."
            );

        }
        catch (InvalidImageContentException)
        {
            return new Result
            (
                type: Result.ResultType.InvalidImageContent,
                defaultMessage: $"Image content is invalid."
            );

        }

        return new Result
            (
                type: Result.ResultType.Success,
                defaultMessage: "Image is a valid image"
            );

    }
    public readonly struct Result(Result.ResultType type, string defaultMessage)
    {
        public ResultType Type { get; } = type;
        public string DefaultMessage { get; } = defaultMessage;
        public enum ResultType
        {
            Success,
            ContentTypeIsNotImage,
            LengthIsNotWithinLimits,
            InvalidImageFormat,
            InvalidImageContent
        }

    }

}
