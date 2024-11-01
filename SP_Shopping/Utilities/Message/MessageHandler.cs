using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NuGet.Protocol;

namespace SP_Shopping.Utilities.Message;

public class MessageHandler : IMessageHandler
{
    private string _messageKey = "Messages";

    public bool PopulateViewDataWithTempData(ITempDataDictionary tempData, ViewDataDictionary viewData)
    {
        if (tempData.TryGetValue(_messageKey, out object? messages) && messages is not null and string)
        {
            viewData[_messageKey] = ((string)messages).FromJson<IEnumerable<Message>>();
            return true;
        }
        else
        {
            return false;
        }
    }

    public void PopulateTempData(ITempDataDictionary tempData, IEnumerable<Message> messages)
    {
        tempData[_messageKey] = messages.ToJson();
    }

    public IEnumerable<Message>? GetMessagesFromViewData(ViewDataDictionary viewData)
    {
        return (IEnumerable<Message>?)viewData[_messageKey];
    }

}
