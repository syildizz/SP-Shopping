using Microsoft.AspNetCore.Mvc;

namespace SP_Shopping.Utilities.ImageValidator;

public class ImageValidator(long maxFileSizeByte = 1_500_000) : IImageValidator
{

    public long MaxFileSizeByte { get; } = maxFileSizeByte;

    public ImageValidatorResult Validate(IFormFile imageFile)
    {

        if (!imageFile.ContentType.Contains("image"))
        {
            return new ImageValidatorResult
            (
                type: ImageValidatorResultType.ContentTypeIsNotImage,
                defaultMessage: "File has to be an image."
            );
        }

        if (imageFile.Length > MaxFileSizeByte)
        {
            return new ImageValidatorResult
            (
                type: ImageValidatorResultType.LengthIsNotWithinLimits,
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
            return new ImageValidatorResult
            (
                type: ImageValidatorResultType.InvalidImageFormat,
                defaultMessage: $"Image format is invalid."
            );

        }
        catch (InvalidImageContentException)
        {
            return new ImageValidatorResult
            (
                type: ImageValidatorResultType.InvalidImageContent,
                defaultMessage: $"Image content is invalid."
            );

        }

        return new ImageValidatorResult
            (
                type: ImageValidatorResultType.Success,
                defaultMessage: "Image is a valid image"
            );

    }
}
