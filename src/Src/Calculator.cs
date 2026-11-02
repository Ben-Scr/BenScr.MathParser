using System;

namespace BenScr.MathParser;

/// <summary>Stateless calculator. Each call uses a fresh evaluator and is safe for concurrent use.</summary>
public static class Calculator
{
    /// <summary>Evaluates as IEEE 754 double; division by zero and invalid domains may yield infinity or NaN.</summary>
    /// <exception cref="ArgumentNullException">Calculation is null.</exception>
    /// <exception cref="ArgumentException">Calculation is empty or whitespace.</exception>
    /// <exception cref="MathParseException">Syntax or resource limits are invalid.</exception>
    /// <exception cref="ExpressionEvaluationException">Names or built-in arguments are invalid.</exception>
    public static double Evaluate(string calculation) => Evaluate<double>(calculation);

    /// <summary>Evaluates and converts using Value.To. Integral conversions round ties to even.</summary>
    /// <remarks>String conversions use the current culture. Each call starts with ans equal to zero.</remarks>
    public static T Evaluate<T>(string calculation)
    {
        ArgumentNullException.ThrowIfNull(calculation);
        if (string.IsNullOrWhiteSpace(calculation))
        {
            throw new ArgumentException("Calculation is empty.", nameof(calculation));
        }
        return ParserRuntime.RunOrThrow(calculation, Evaluator.CreateCalculator()).To<T>();
    }
}
