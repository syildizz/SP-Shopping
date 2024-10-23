namespace SP_Shopping.Utilities.ImageValidator;

public readonly struct ImageValidatorResult(ResultType type, string defaultMessage)
{
    public ResultType Type { get; } = type;
    public string DefaultMessage { get; } = defaultMessage;
}
