using System;
using System.Collections.Generic;

namespace BenScr.MathParser;

/// <summary>Evaluates complete expressions and updates a caller-owned evaluator's ans on success.</summary>
public static class ParserRuntime
{
    /// <summary>Returns the result, or a string Value starting with "Error: " for expression errors.</summary>
    /// <remarks>Null arguments and unrelated callback exceptions propagate. Evaluators require external synchronization.</remarks>
    public static Value Run(string src, Evaluator ev) =>
        TryRun(src, ev, out Value result, out string? error) ? result : new Value("Error: " + error);

    /// <summary>Evaluates complete input. A failed evaluation leaves ans unchanged.</summary>
    /// <exception cref="ArgumentNullException">Input or evaluator is null.</exception>
    /// <exception cref="MathParseException">Syntax, length, or depth is invalid.</exception>
    /// <exception cref="ExpressionEvaluationException">A name or built-in argument contract is invalid.</exception>
    public static Value RunOrThrow(string src, Evaluator ev)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(ev);
        List<Token> tokens = Lexer.BuildTokens(src);
        var parser = new Parser(tokens);
        Expr expression = parser.ParseExpression();
        parser.ExpectEndOfInput();
        Value result = ev.EvalToValue(expression);
        ev.SetVariable("ans", result);
        return result;
    }

    /// <summary>Returns false for parser or built-in evaluation errors, with a null Value and descriptive error.</summary>
    /// <remarks>Null arguments and unrelated exceptions from custom callbacks propagate.</remarks>
    public static bool TryRun(string src, Evaluator ev, out Value result, out string? error)
    {
        ArgumentNullException.ThrowIfNull(src);
        ArgumentNullException.ThrowIfNull(ev);
        try
        {
            result = RunOrThrow(src, ev);
            error = null;
            return true;
        }
        catch (Exception exception) when (exception is MathParseException or ExpressionEvaluationException)
        {
            result = Value.Null;
            error = exception.Message;
            return false;
        }
    }
}
