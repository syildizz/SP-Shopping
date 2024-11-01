using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NuGet.Protocol;

namespace SP_Shopping.Utilities.Message;

public class MessageHandler : IMessageHandler
{
    public readonly string _messageKey = "Messages";

    public void AddMessages(ITempDataDictionary tempData, IEnumerable<Message> messages)
    {
        if (tempData.TryGetValue(_messageKey, out object? _messages) && _messages is not null and string)
        {
            var alreadyExistingMessages = ((string)_messages).FromJson<IEnumerable<Message>>();
            tempData[_messageKey] = alreadyExistingMessages.Concat(messages).ToJson();
        }
        else
        {
            tempData[_messageKey] = messages.ToJson();
        }
    }

    public IEnumerable<Message>? GetMessages(ITempDataDictionary tempData)
    {
        if (tempData.TryGetValue(_messageKey, out object? messages) && messages is not null and string)
        {
            return ((string)messages).FromJson<IEnumerable<Message>>();
        }
        else
        {
            return null;
        }
    }

}
