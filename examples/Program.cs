using System;
using System.Globalization;
using BenScr.MathParser;

CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
Check(Calculator.Evaluate("20 + 100 / 2 * 4^2") == 820, "precedence");
Check(Calculator.Evaluate("2^3^2") == 512, "power associativity");
Check(Calculator.Evaluate("1,5 + 1.5") == 3, "decimal separators");
Check(Calculator.Evaluate<string>("2$ + 5$") == "7.00$", "currency labels");
Evaluator evaluator = Evaluator.CreateCalculator();
evaluator.SetVariable("x", new Value(21d));
evaluator.Define("twice", 1, values => new Value(values[0].To<double>() * 2));
Check(ParserRuntime.RunOrThrow("twice(x)", evaluator).To<double>() == 42, "custom function");
Check(ParserRuntime.RunOrThrow("ans + 1", evaluator).To<double>() == 43, "session state");
Check(!ParserRuntime.TryRun("1 +", evaluator, out _, out string? error) && error != null, "syntax error");
try
{
    Calculator.Evaluate("1 + @");
    throw new InvalidOperationException("Expected a syntax error.");
}
catch (MathParseException exception)
{
    Check(exception.Position == 4, "error position");
}
Console.WriteLine("All package consumer scenarios passed.");

static void Check(bool condition, string scenario)
{
    if (!condition)
    {
        throw new InvalidOperationException($"Failed: {scenario}.");
    }
    Console.WriteLine($"Passed: {scenario}");
}
