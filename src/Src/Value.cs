using System;
using System.Globalization;

namespace BenScr.MathParser;

/// <summary>A shallow immutable wrapper. Wrapped custom objects retain their own ownership and thread-safety rules.</summary>
public readonly struct Value
{
    /// <summary>Gets the borrowed wrapped object; no copy is made.</summary>
    public object? Object { get; }

    /// <summary>Wraps an object, or null.</summary>
    public Value(object? obj = null) => Object = obj;

    /// <summary>The null expression value.</summary>
    public static readonly Value Null = new(null);

    /// <summary>Converts using Convert.ChangeType and the current culture, or returns an already compatible object.</summary>
    /// <remarks>Currency converts to its amount except for string or CurrencyAmount targets. Integers round ties to even.
    /// Null converts only to nullable targets. Invalid conversions throw InvalidCastException, FormatException or OverflowException.</remarks>
    public T To<T>()
    {
        if (Object is T exact)
        {
            return exact;
        }
        Type targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        if (Object is CurrencyAmount currency)
        {
            if (targetType == typeof(string))
            {
                return (T)(object)currency.ToString();
            }
            return (T)Convert.ChangeType(currency.Amount, targetType, CultureInfo.CurrentCulture);
        }
        if (Object == null)
        {
            if (!typeof(T).IsValueType || Nullable.GetUnderlyingType(typeof(T)) != null)
            {
                return default!;
            }
            throw new InvalidCastException($"Cannot convert null to {typeof(T).Name}.");
        }
        return (T)Convert.ChangeType(Object, targetType, CultureInfo.CurrentCulture);
    }

    /// <summary>Formats using the wrapped object's ToString; null is rendered as "null".</summary>
    public override string ToString() => Object?.ToString() ?? "null";
}
