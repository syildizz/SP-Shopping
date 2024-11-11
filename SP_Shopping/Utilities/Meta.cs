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
