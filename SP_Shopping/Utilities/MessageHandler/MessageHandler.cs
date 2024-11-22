using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using NuGet.Protocol;

namespace SP_Shopping.Utilities.MessageHandler;

public class MessageHandler(string messageKey = "Messages") : IMessageHandler
{
    private readonly string _messageKey = messageKey;

    public void Add(ITempDataDictionary tempData, Message message)
    {
        Add(tempData, [message]);
    }

    public void Add(ITempDataDictionary tempData, IEnumerable<Message> messages)
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

    public IEnumerable<Message>? Get(ITempDataDictionary tempData)
    {
        if (tempData.TryGetValue(_messageKey, out object? messages))
        {
            return (messages as string)?.FromJson<IEnumerable<Message>>();
        }
        else
        {
            return null;
        }
    }

    public IEnumerable<Message>? Peek(ITempDataDictionary tempData)
    {
        var messages = tempData.Peek(_messageKey);
        return (messages as string)?.FromJson<IEnumerable<Message>>();
    }

}
