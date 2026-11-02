namespace BenScr.MathParser;

/// <summary>An approximate amount with a display label; not a financial or currency-conversion type.</summary>
/// <param name="Amount">The IEEE 754 numeric amount.</param>
/// <param name="Symbol">The display label; arithmetic preserves the first currency label.</param>
public readonly record struct CurrencyAmount(double Amount, string Symbol)
{
    /// <summary>Formats with current-culture N2 (two decimals and grouping), then appends the symbol.</summary>
    public override string ToString() => $"{Amount:N2}{Symbol}";
}
