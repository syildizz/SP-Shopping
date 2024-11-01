using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Web;

namespace SP_Shopping.Utilities.Message;

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
            MessageType.Success => BaseContentHtml("success"),
            MessageType.Info => BaseContentHtml("info"),
            MessageType.Error => BaseContentHtml("danger"),
            MessageType.Warning => BaseContentHtml("warning"),
            _ => 
                throw new NotImplementedException("MessageType does not exist")


        };
    }

    private string BaseContentHtml(string s)
    {
        return 
        $"""
            <div class="alert alert-{s} alert-dismissible fade show" role="alert">
                {HttpUtility.HtmlEncode(Content)}
                <button type="button" class="btn-close" data-bs-dismiss="alert" aria-label="Close">
                </button>
            </div>
        """;
    }

}
