using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace SP_Shopping.Utilities.Message;
public interface IMessageHandler
{
    IEnumerable<Message>? GetMessagesFromViewData(ViewDataDictionary viewData);
    void PopulateTempData(ITempDataDictionary tempData, IEnumerable<Message> messages);
    bool PopulateViewDataWithTempData(ITempDataDictionary tempData, ViewDataDictionary viewData);
}