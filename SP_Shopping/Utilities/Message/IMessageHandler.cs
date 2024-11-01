using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SP_Shopping.Utilities.Message;
public interface IMessageHandler
{
    IEnumerable<Message>? GetMessages(ITempDataDictionary tempData);
    void AddMessages(ITempDataDictionary tempData, IEnumerable<Message> messages);
}