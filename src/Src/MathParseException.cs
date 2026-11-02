using System;

namespace BenScr.MathParser;

/// <summary>A syntax or limit error at a zero-based UTF-16 source offset.</summary>
public sealed class MathParseException : FormatException
{
    /// <summary>Creates an error at the given position; input length denotes EOF.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The position is negative.</exception>
    public MathParseException(string message, int position) : base($"{message} at position {position}.")
    {
        ArgumentOutOfRangeException.ThrowIfNegative(position);
        Position = position;
    }

    /// <summary>Gets the zero-based UTF-16 source offset.</summary>
    public int Position { get; }
}
