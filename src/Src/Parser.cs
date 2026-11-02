using System;
using System.Collections.Generic;
using System.Globalization;

namespace BenScr.MathParser;

/// <summary>Parses a token snapshot. Instances require external synchronization.</summary>
public sealed class Parser
{
    /// <summary>The maximum recursive parsing depth and expression-tree depth, inclusive.</summary>
    public const int MaximumDepth = 128;
    private readonly Token[] tokens;
    private int position;
    private int depth;

    /// <summary>Copies a bounded token list with nonnegative positions and exactly one final EOF.</summary>
    /// <exception cref="ArgumentNullException">Tokens are null.</exception>
    /// <exception cref="ArgumentException">The token sequence is invalid.</exception>
    public Parser(List<Token> tokens)
    {
        ArgumentNullException.ThrowIfNull(tokens);
        if (tokens.Count == 0 || tokens.Count > Lexer.MaximumInputLength + 1 || tokens[^1].Type != TokenType.EOF)
        {
            throw new ArgumentException("Tokens must be bounded and end with EOF.", nameof(tokens));
        }
        foreach (Token token in tokens)
        {
            if (token.Lexeme == null || token.Pos < 0 || !Enum.IsDefined(token.Type))
            {
                throw new ArgumentException("Invalid token.", nameof(tokens));
            }
        }
        for (int i = 0; i < tokens.Count - 1; i++)
        {
            if (tokens[i].Type == TokenType.EOF)
            {
                throw new ArgumentException("Only the final token may be EOF.", nameof(tokens));
            }
        }
        this.tokens = tokens.ToArray();
    }

    /// <summary>Requires EOF at the current position; trailing input raises MathParseException.</summary>
    public void ExpectEndOfInput() => Expect(TokenType.EOF);

    /// <summary>Parses one expression with a minimum binding power; use zero for complete expressions.</summary>
    /// <remarks>Call ExpectEndOfInput to reject trailing tokens. Unary operators bind tighter than power.</remarks>
    /// <exception cref="ArgumentOutOfRangeException">The binding power is negative.</exception>
    /// <exception cref="MathParseException">Syntax or depth is invalid.</exception>
    public Expr ParseExpression(int rbp = 0)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(rbp);
        Token first = Peek();
        if (++depth > MaximumDepth)
        {
            depth--;
            throw new MathParseException($"Expression exceeds depth {MaximumDepth}", first.Pos);
        }
        try
        {
            Expr left = ParsePrefix(Next());
            while (rbp < BindingPower(Peek().Type))
            {
                Token token = Next();
                left = token.Type == TokenType.Currency
                    ? new MoneyExpr(left, token.Lexeme)
                    : new BinaryExpr(token.Type, left, ParseExpression(token.Type == TokenType.Caret ? 29 : BindingPower(token.Type)));
            }
            if (depth == 1)
            {
                ValidateTreeDepth(left);
            }
            return left;
        }
        finally
        {
            depth--;
        }
    }

    private Token Peek() => tokens[System.Math.Min(position, tokens.Length - 1)];

    private Token Next()
    {
        Token token = Peek();
        if (position < tokens.Length - 1)
        {
            position++;
        }
        return token;
    }

    private bool Match(TokenType type)
    {
        if (Peek().Type != type)
        {
            return false;
        }
        Next();
        return true;
    }

    private void Expect(TokenType type)
    {
        Token token = Next();
        if (token.Type != type)
        {
            throw new MathParseException($"Expected {type}, found {token.Type}", token.Pos);
        }
    }

    private Expr ParsePrefix(Token token)
    {
        switch (token.Type)
        {
            case TokenType.Number:
                if (!double.TryParse(token.Lexeme.Replace(',', '.'), NumberStyles.AllowDecimalPoint,
                    CultureInfo.InvariantCulture, out double value))
                {
                    throw new MathParseException("Invalid number", token.Pos);
                }
                return new NumberExpr(value);
            case TokenType.String:
                return new StringExpr(token.Lexeme);
            case TokenType.Ident:
                if (!Match(TokenType.LParen))
                {
                    return new VarExpr(token.Lexeme);
                }
                var arguments = new List<Expr>();
                if (!Match(TokenType.RParen))
                {
                    do
                    {
                        arguments.Add(ParseExpression());
                        if (Match(TokenType.RParen))
                        {
                            return new CallExpr(token.Lexeme, arguments);
                        }
                        Expect(TokenType.Separator);
                    } while (true);
                }
                return new CallExpr(token.Lexeme, arguments);
            case TokenType.LParen:
                Expr grouped = ParseExpression();
                Expect(TokenType.RParen);
                return grouped;
            case TokenType.Plus:
            case TokenType.Minus:
            case TokenType.Sqrt:
                return new UnaryExpr(token.Type, ParseExpression(70));
            default:
                throw new MathParseException($"Unexpected token {token.Type}", token.Pos);
        }
    }

    private static int BindingPower(TokenType type) => type switch
    {
        TokenType.Currency => 80,
        TokenType.Caret => 30,
        TokenType.Star or TokenType.Slash or TokenType.Modulo => 20,
        TokenType.Plus or TokenType.Minus => 10,
        _ => 0
    };

    private void ValidateTreeDepth(Expr root)
    {
        // Long left-associative chains grow the tree without increasing parser recursion.
        var pending = new Stack<(Expr Node, int Depth)>();
        pending.Push((root, 1));
        while (pending.TryPop(out var item))
        {
            if (item.Depth > MaximumDepth)
            {
                throw new MathParseException($"Expression tree exceeds depth {MaximumDepth}", Peek().Pos);
            }
            switch (item.Node)
            {
                case UnaryExpr unary: pending.Push((unary.Right, item.Depth + 1)); break;
                case MoneyExpr money: pending.Push((money.Left, item.Depth + 1)); break;
                case BinaryExpr binary:
                    pending.Push((binary.Left, item.Depth + 1));
                    pending.Push((binary.Right, item.Depth + 1));
                    break;
                case CallExpr call:
                    foreach (Expr argument in call.Args)
                    {
                        pending.Push((argument, item.Depth + 1));
                    }
                    break;
            }
        }
    }
}
