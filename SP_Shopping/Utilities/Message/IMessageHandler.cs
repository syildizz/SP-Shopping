using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SP_Shopping.Utilities.Message;
public interface IMessageHandler
{
    IEnumerable<Message>? Get(ITempDataDictionary tempData);
    void Add(ITempDataDictionary tempData, IEnumerable<Message> messages);

    void Add(ITempDataDictionary tempData, Message message);
    IEnumerable<Message>? Peek(ITempDataDictionary tempData);
}