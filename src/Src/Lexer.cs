using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Globalization;

namespace BenScr.MathParser;

/// <summary>Tokenizes expressions. Calls are independent and safe for concurrent use.</summary>
public static class Lexer
{
    /// <summary>The maximum accepted input length in UTF-16 code units, inclusive.</summary>
    public const int MaximumInputLength = 16384;

    /// <summary>Gets the current culture's separator for display. Parsing accepts dot and comma.</summary>
    public static char FloatingPointSeparator => CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0];

    /// <summary>Gets the other accepted decimal separator.</summary>
    public static char OppositeFloatingPointSeparator => FloatingPointSeparator == ',' ? '.' : ',';

    /// <summary>Gets the immutable set of supported currency labels.</summary>
    public static IReadOnlySet<char> Currencies { get; } = new[] { '€', '$' }.ToFrozenSet();

    /// <summary>Tokenizes complete input, including a final EOF. Dot and comma are decimals, not grouping separators.</summary>
    /// <exception cref="ArgumentNullException">The source is null.</exception>
    /// <exception cref="MathParseException">The input is too long or contains invalid characters or an unterminated string.</exception>
    public static List<Token> BuildTokens(string source)
    {
        ArgumentNullException.ThrowIfNull(source);
        if (source.Length > MaximumInputLength)
        {
            throw new MathParseException($"Input exceeds {MaximumInputLength} characters", MaximumInputLength);
        }
        var tokens = new List<Token>();
        int i = 0;
        while (i < source.Length)
        {
            char c = source[i];
            if (char.IsWhiteSpace(c))
            {
                i++;
                continue;
            }
            if (char.IsAsciiDigit(c) || ((c == '.' || c == ',') && i + 1 < source.Length && char.IsAsciiDigit(source[i + 1])))
            {
                int start = i;
                bool hasDecimalSeparator = false;
                while (i < source.Length)
                {
                    char digit = source[i];
                    if (digit == '.' || digit == ',')
                    {
                        if (hasDecimalSeparator)
                        {
                            break;
                        }
                        hasDecimalSeparator = true;
                        i++;
                    }
                    else if (char.IsAsciiDigit(digit))
                    {
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
                tokens.Add(new Token(TokenType.Number, source[start..i], start));
                continue;
            }
            if (c == '"')
            {
                int start = ++i;
                while (i < source.Length && source[i] != '"')
                {
                    i++;
                }
                if (i == source.Length)
                {
                    throw new MathParseException("Unterminated string", start - 1);
                }
                tokens.Add(new Token(TokenType.String, source[start..i], start - 1));
                i++;
                continue;
            }
            if (char.IsLetter(c) || c == '_')
            {
                int start = i++;
                while (i < source.Length && (char.IsLetterOrDigit(source[i]) || source[i] == '_'))
                {
                    i++;
                }
                tokens.Add(new Token(TokenType.Ident, source[start..i], start));
                continue;
            }
            TokenType type = c switch
            {
                '+' => TokenType.Plus,
                '-' => TokenType.Minus,
                '*' => TokenType.Star,
                '/' => TokenType.Slash,
                '%' => TokenType.Modulo,
                '^' => TokenType.Caret,
                '(' => TokenType.LParen,
                ')' => TokenType.RParen,
                ';' => TokenType.Separator,
                '√' => TokenType.Sqrt,
                '$' or '€' => TokenType.Currency,
                _ => throw new MathParseException($"Unexpected character '{c}'", i)
            };
            tokens.Add(new Token(type, c.ToString(), i++));
        }
        tokens.Add(new Token(TokenType.EOF, string.Empty, source.Length));
        return tokens;
    }
}
