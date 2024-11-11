
namespace SP_Shopping.Utilities.ImageValidator;

public interface IImageValidator
{
    long MaxFileSizeByte { get; }

    ImageValidatorResult Validate(IFormFile imageFile);
}