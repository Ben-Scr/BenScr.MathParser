using System;

namespace BenScr.MathParser;

/// <summary>An unknown expression name or invalid built-in function argument.</summary>
public sealed class ExpressionEvaluationException : InvalidOperationException
{
    /// <summary>Creates an evaluation error describing the failed contract.</summary>
    public ExpressionEvaluationException(string message) : base(message) { }

    /// <summary>Creates an error retaining the original conversion or range error.</summary>
    public ExpressionEvaluationException(string message, Exception innerException) : base(message, innerException) { }
}
