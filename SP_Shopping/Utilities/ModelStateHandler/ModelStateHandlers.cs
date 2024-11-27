using Microsoft.AspNetCore.Mvc.ModelBinding;
using NuGet.Protocol;

namespace SP_Shopping.Utilities.ModelStateHandler;

// https://andrewlock.net/post-redirect-get-using-tempdata-in-asp-net-core/

public static class ModelStateHandlers
{
    private class ModelStateTransferValue
    {
        public required string Key { get; set; }
        public string? AttemptedValue { get; set; }
        public object? RawValue { get; set; }
        public ICollection<string>? ErrorMessages { get; set; } = [];
    }


    public static string SerialiseModelState(ModelStateDictionary modelState)
    {
        var errorList = modelState
            .Select(kvp => new ModelStateTransferValue
            {
                Key = kvp.Key,
                AttemptedValue = kvp.Value?.AttemptedValue,
                RawValue = kvp.Value?.RawValue,
                ErrorMessages = kvp.Value?.Errors.Select(err => err.ErrorMessage).ToList(),
            });

        return errorList.ToJson();
    }

    public static ModelStateDictionary DeserialiseModelState(string serialisedErrorList)
    {
        var errorList = serialisedErrorList.FromJson<List<ModelStateTransferValue>>();
        var modelState = new ModelStateDictionary();

        foreach (var item in errorList)
        {
            modelState.SetModelValue(item.Key, item.RawValue, item.AttemptedValue);
            if (item.ErrorMessages is not null)
            {
                foreach (var error in item.ErrorMessages)
                {
                    modelState.AddModelError(item.Key, error);
                }
            }
        }
        return modelState;
    }
}

