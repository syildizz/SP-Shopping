using SP_Shopping.Utilities.ImageValidator;
using System.ComponentModel.DataAnnotations;

namespace SP_Shopping.Attributes;

public class IsImageFileAttribute(long maxFileSizeByte = 1_500_000) : ValidationAttribute
{

    private readonly ImageValidator _imageValidator = new(maxFileSizeByte);

    public override bool IsValid(object? value)
    {
        if (value is IFormFile formFile)
        {
            var result = _imageValidator.Validate(formFile);
            if (result.Type is not ImageValidator.Result.ResultType.Success)
            {
                ErrorMessage = result.DefaultMessage;
                return false;
            }
            else
            {
                return true;
            }

        }
        else if (value is null)
        {
            return true;
        }
        else
        {
            ErrorMessage = $"{nameof(IsImageFileAttribute)} attribute must only be applied to {nameof(IFormFile)} type.";
            return false;
        }

    }

}
