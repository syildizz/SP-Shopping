using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Web;

namespace SP_Shopping.Utilities.MessageHandler;

public class Message
{

    public enum MessageType
    {
        Success,
        Info,
        Error,
        Warning
    }

    public required MessageType Type { get; set; }
    public required string Content { get; set; }

    public string ContentHtml
    {
        get => Type switch
        {
            MessageType.Success => BaseContentHtml("alert-success", "bi-check-circle-fill"),
            MessageType.Info => BaseContentHtml("alert-info", "bi-info-circle-fill"),
            MessageType.Error => BaseContentHtml("alert-danger", "bi-exclamation-triangle-fill"),
            MessageType.Warning => BaseContentHtml("alert-warning", "bi-exclamation-triangle-fill"),
            _ =>
                throw new NotImplementedException("MessageType does not exist")
        };
    }

    private string BaseContentHtml(string alertType, string iconType)
    {
        //return 
        //$"""
        //    <div class="alert alert-{s} alert-dismissible fade show" role="alert">
        //        {HttpUtility.HtmlEncode(Content)}
        //        <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
        //        </button>
        //    </div>
        //""";
        return
        $"""
        <div class="alert {alertType} fade show d-flex align-items-center justify-content-between" role="alert">
            <div>
                <i class="bi {iconType} flex-shrink-0 me-2"></i>
                {HttpUtility.HtmlEncode(Content)}
            </div>
            <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
            </button>
        </div>
        """;
    }

}
