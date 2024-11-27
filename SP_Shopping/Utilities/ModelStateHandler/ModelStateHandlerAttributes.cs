using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace SP_Shopping.Utilities.ModelStateHandler;

// https://andrewlock.net/post-redirect-get-using-tempdata-in-asp-net-core/

public abstract class ModelStateTransfer : ActionFilterAttribute
{
    protected const string Key = nameof(ModelStateTransfer);
}

public class ExportModelStateAttribute : ModelStateTransfer
{
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        //Only export when ModelState is not valid
        if (!filterContext.ModelState.IsValid)
        {
            //Export if we are redirecting
            if (filterContext.Result is RedirectResult 
                || filterContext.Result is RedirectToRouteResult 
                || filterContext.Result is RedirectToActionResult)
            {
                if (filterContext.Controller is Controller controller && filterContext.ModelState != null)
                {
                    var modelState = ModelStateHandlers.SerialiseModelState(filterContext.ModelState);
                    controller.TempData[Key] = modelState;
                }
            }
        }

        base.OnActionExecuted(filterContext);
    }
}

public class ImportModelStateAttribute : ModelStateTransfer
{
    public override void OnActionExecuted(ActionExecutedContext filterContext)
    {
        var controller = filterContext.Controller as Controller;

        if (controller?.TempData[Key] is string serialisedModelState)
        {
            //Only Import if we are viewing
            if (filterContext.Result is ViewResult)
            {
                var modelState = ModelStateHandlers.DeserialiseModelState(serialisedModelState);
                filterContext.ModelState.Merge(modelState);
            }
            else
            {
                //Otherwise remove it.
                controller.TempData.Remove(Key);
            }
        }

        base.OnActionExecuted(filterContext);
    }
}

