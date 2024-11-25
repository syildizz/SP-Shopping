
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;

namespace SP_Shopping.Test.TestingUtilities;

internal static class AttributeHandler
{

    internal static bool HasAuthorizationAttributes(Type controller, MethodInfo? action, string? checkRole = null, string? checkPolicy = null)
    {
        if (controller.GetCustomAttribute(typeof(AuthorizeAttribute), false) is AuthorizeAttribute attribute)
        {
            if (action?.GetCustomAttribute(typeof(AllowAnonymousAttribute), false) is not AllowAnonymousAttribute)
            {
                bool result = true;
                if (checkRole is not null)
                {
                    result = result && (attribute.Roles?.Contains(checkRole) ?? false);
                }
                if (checkPolicy is not null)
                {
                    result = result && (attribute.Policy?.Contains(checkPolicy) ?? false);
                }
                return result;
            }
        }
        else
        {
            if (action?.GetCustomAttribute(typeof(AuthorizeAttribute), false) is AuthorizeAttribute attribute2)
            {
                bool result = true;
                if (checkRole is not null)
                {
                    result = result && (attribute2.Roles?.Contains(checkRole) ?? false);
                }
                if (checkPolicy is not null)
                {
                    result = result && (attribute2.Policy?.Contains(checkPolicy) ?? false);
                }
                return result;
            }
        }
        return false;
    }

}
