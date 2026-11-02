
namespace BenScr.MathParser;

/// <summary>The supported lexical token categories. Historical numeric values and aliases are retained.</summary>
public enum TokenType
{
    /// <summary>
    /// A numeric literal
    /// </summary>
    Number,

    /// <summary>
    /// A string literal defined with double quotes
    /// </summary>
    String,

    /// <summary>
    /// An identifier such as a variable or function name
    /// </summary>
    Ident,

    /// <summary>
    /// Addition operator +
    /// </summary>
    Plus,

    /// <summary>
    /// Subtraction operator -
    /// </summary>
    Minus,

    /// <summary>
    /// Exponentiation operator ^
    /// </summary>
    Caret,

    /// <summary>
    /// Multiplication operator *
    /// </summary>
    Star,

    /// <summary>
    /// Division operator /
    /// </summary>
    Slash,

    /// <summary>
    /// Modulo operator %
    /// </summary>
    Modulo,

    /// <summary>
    /// Left Parenthesis (
    /// </summary>
    LParen,

    /// <summary>
    /// Right Parenthesis )
    /// </summary>
    RParen,

    /// <summary>
    /// Separator for function arguments ;.
    /// </summary>
    Seperator,

    /// <summary>
    /// Separator for function arguments ;.
    /// </summary>
    Separator = Seperator,

    /// <summary>
    /// Sqrt operator √
    /// </summary>
    Sqrt,

    /// <summary>
    /// Indicates whether a value uses a currency symbol like $, €
    /// </summary>
    Currency,

    /// <summary>
    /// Indicates the end of the input
    /// </summary>
    EOF
}

/// <summary>An immutable token with a zero-based UTF-16 source position.</summary>
public readonly struct Token
{
    /// <summary>The lexical category.</summary>
    public readonly TokenType Type;
    /// <summary>The source lexeme; string tokens omit their quotes.</summary>
    public readonly string Lexeme;
    /// <summary>The zero-based UTF-16 source offset.</summary>
    public readonly int Pos;

    /// <summary>Creates a token. Parser validates token sequences before use.</summary>
    public Token(TokenType type, string lexeme, int pos)
    {
        Type = type;
        Lexeme = lexeme;
        Pos = pos;
    }

    /// <summary>Formats the category and lexeme for diagnostics.</summary>
    public override string ToString() => $"{Type}:{Lexeme}";
}
