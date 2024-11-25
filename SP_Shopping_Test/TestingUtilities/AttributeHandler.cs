
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;

namespace SP_Shopping.Test.TestingUtilities;

internal static class AttributeHandler
{
    internal static bool HasAuthorizationAttributes(Type controller, MethodInfo? action)
    {
        return
        !(action?.GetCustomAttributes(typeof(AuthorizeAttribute), true).IsNullOrEmpty() ?? false)
        ||

            !(controller?.GetCustomAttributes(typeof(AuthorizeAttribute), true).IsNullOrEmpty() ?? false)
            &&
            (action?.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).IsNullOrEmpty() ?? false)
        ;
    }

    internal static bool HasAuthorizationAttributes(Type controller)
    {
        return !(controller?.GetCustomAttributes(typeof(AuthorizeAttribute), true).IsNullOrEmpty() ?? false);
    }
}
