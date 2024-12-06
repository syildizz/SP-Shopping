using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace SP_Shopping.Utilities;

public static class Meta
{
    /// <summary>
    ///     Used for calling function like a method
    ///     e.g. { b res = f(a); }
    ///          is equivalent to
    ///          { b res = a._(f); }
    /// </summary>
    public static B _<A, B>(this A _, Func<A, B> f)
    {
        return f(_);
    }

}

// https://stackoverflow.com/a/57707700
public static class TupleExtensions
{
    public static bool TryOut<P2>(this ValueTuple<bool, P2> tuple, out P2 p2)
    {
        bool p1;
        (p1, p2) = tuple;
        return p1;
    }

    public static bool TryOut<P2, P3>(this ValueTuple<bool, P2, P3> tuple, out P2 p2, out P3 p3)
    {
        bool p1;
        (p1, p2, p3) = tuple;
        return p1;
    }

    // continue to support larger tuples...
}

public static class ModelStateErrorMessagesToListOfMessages
{
    public static IEnumerable<string> GetErrorMessages(this ModelStateDictionary modelState)
    {
        return modelState
            .Where(ms =>
                ms.Value is not null
                &&
                ms.Value.ValidationState is ModelValidationState.Invalid
            )
            .SelectMany(ms => ms.Value!.Errors
                .Select(es => es.ErrorMessage)
            );
    }
}
