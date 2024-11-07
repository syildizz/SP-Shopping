namespace SP_Shopping.Utilities.ImageValidator;

public readonly struct ImageValidatorResult(ImageValidatorResultType type, string defaultMessage)
{
    public ImageValidatorResultType Type { get; } = type;
    public string DefaultMessage { get; } = defaultMessage;
}
