using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text.RegularExpressions;

namespace SP_Shopping.Utilities.Attributes;

// Following code is taken from:
// https://stackoverflow.com/a/78150423 by Michael Ulloa
// It is licensed under the terms of CC BY-SA 4.0 according to:
// https://stackoverflow.com/help/licensing
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
public class PrecisionAndScaleAttribute : ValidationAttribute
{
    private readonly int _precision;
    private readonly int _scale;

    public PrecisionAndScaleAttribute(int precision, int scale)
        : base(() => "The field {0} only allows decimals with precision {1} and scale {2}.")
    {
        _precision = precision;
        _scale = scale;
    }

    public override bool IsValid(object? value)
    {
        if (value is null)
            return true;

        if (value is not decimal decimalValue)
            return false;

        string? precisionValue = decimalValue.ToString(CultureInfo.InvariantCulture);

        return precisionValue is null || Regex.IsMatch(precisionValue, $@"^(0|-?\d{{0,{_precision - _scale}}}(\.\d{{0,{_scale}}})?)$");
    }

    /// <summary>
    /// Override of <see cref="ValidationAttribute.FormatErrorMessage"/>
    /// </summary>
    /// <param name="name">The user-visible name to include in the formatted message.</param>
    public override string FormatErrorMessage(string name)
        => string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, _precision, _scale);
}

