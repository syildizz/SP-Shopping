using Microsoft.AspNetCore.Mvc;

namespace SP_Shopping.Utilities.ImageValidator;

public class ImageValidator(long maxFileSizeByte = 1_500_000) : IImageValidator
{

    public long MaxFileSizeByte { get; } = maxFileSizeByte;

    public ImageValidatorResult Validate(IFormFile maybeImageFile)
    {

        if (!maybeImageFile.ContentType.Contains("image"))
        {
            return new ImageValidatorResult
            (
                type: ResultType.ContentTypeIsNotImage,
                defaultMessage: "File has to be an image."
            );
        }

        if (maybeImageFile.Length > MaxFileSizeByte)
        {
            return new ImageValidatorResult
            (
                type: ResultType.LengthIsNotWithinLimits,
                defaultMessage: $"Cannot upload images larger than {MaxFileSizeByte} bytes to the database."
            );
        }

        try
        {
            Image.DetectFormat(maybeImageFile.OpenReadStream());
        }
        catch (UnknownImageFormatException)
        {
            return new ImageValidatorResult
            (
                type: ResultType.InvalidImageFormat,
                defaultMessage: $"Image format is invalid."
            );

        }
        catch (InvalidImageContentException)
        {
            return new ImageValidatorResult
            (
                type: ResultType.InvalidImageContent,
                defaultMessage: $"Image content is invalid."
            );

        }

        return new ImageValidatorResult
            (
                type: ResultType.Success,
                defaultMessage: "Image is a valid image"
            );

    }
}
