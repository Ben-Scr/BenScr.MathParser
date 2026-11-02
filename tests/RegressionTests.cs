using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace BenScr.MathParser.Tests;

[TestFixture]
public sealed class RegressionTests
{
    [TestCase("1 +", 3)]
    [TestCase("(1 + 2", 6)]
    [TestCase("1 2", 2)]
    [TestCase("1 + @", 4)]
    [TestCase("\"open", 0)]
    [TestCase("pow(2;)", 6)]
    [TestCase("1.000,5", 5)]
    [TestCase("1 + ١", 4)]
    public void SyntaxErrorsIdentifyOriginalPosition(string input, int expectedPosition)
    {
        MathParseException? error = Assert.Throws<MathParseException>(() => Calculator.Evaluate(input));
        Assert.That(error!.Position, Is.EqualTo(expectedPosition));
    }

    [TestCase("")]
    [TestCase("  ")]
    public void RuntimeReturnsExpressionErrorForEmptyInput(string input)
    {
        Assert.That(ParserRuntime.TryRun(input, Evaluator.CreateCalculator(), out Value value, out string? error), Is.False);
        Assert.That(value.Object, Is.Null);
        Assert.That(error, Does.Contain("position"));
    }

    [Test]
    public void TryRunDoesNotHideProgrammingErrors()
    {
        var evaluator = Evaluator.CreateCalculator();
        Assert.Throws<ArgumentNullException>(() => ParserRuntime.TryRun(null!, evaluator, out _, out _));
        Assert.Throws<ArgumentNullException>(() => ParserRuntime.TryRun("1", null!, out _, out _));
        var failure = new InvalidOperationException("callback failure");
        evaluator.Define("fail", 0, _ => throw failure);
        Assert.That(Assert.Throws<InvalidOperationException>(() =>
            ParserRuntime.TryRun("fail()", evaluator, out _, out _)), Is.SameAs(failure));
        evaluator.Define("badArgument", 0, _ => throw new ArgumentException("callback argument"));
        Assert.Throws<ArgumentException>(() => ParserRuntime.TryRun("badArgument()", evaluator, out _, out _));
    }

    [TestCase("missing")]
    [TestCase("missing(2)")]
    [TestCase("max()")]
    [TestCase("sin(1;2)")]
    [TestCase("clamp(0;2;1)")]
    [TestCase("sqrt(\"text\")")]
    public void ExpectedEvaluationErrorsAreReturnedWithoutChangingAns(string expression)
    {
        var evaluator = Evaluator.CreateCalculator();
        ParserRuntime.RunOrThrow("42", evaluator);
        Assert.That(ParserRuntime.TryRun(expression, evaluator, out _, out string? error), Is.False);
        Assert.That(error, Is.Not.Empty);
        Assert.That(ParserRuntime.RunOrThrow("ans", evaluator).To<double>(), Is.EqualTo(42));
    }

    [Test]
    public void LengthLimitIsInclusive()
    {
        string atLimit = "1" + new string(' ', Lexer.MaximumInputLength - 1);
        Assert.That(Calculator.Evaluate(atLimit), Is.EqualTo(1));
        var exception = Assert.Throws<MathParseException>(() => Calculator.Evaluate(atLimit + " "));
        Assert.That(exception!.Position, Is.EqualTo(Lexer.MaximumInputLength));
    }

    [Test]
    public void NestingAndTreeLimitsAreInclusive()
    {
        Assert.That(Calculator.Evaluate(new string('(', 127) + "1" + new string(')', 127)), Is.EqualTo(1));
        Assert.Throws<MathParseException>(() => Calculator.Evaluate(new string('(', 128) + "1" + new string(')', 128)));
        Assert.That(Calculator.Evaluate(string.Join("+", Enumerable.Repeat("1", 128))), Is.EqualTo(128));
        Assert.Throws<MathParseException>(() => Calculator.Evaluate(string.Join("+", Enumerable.Repeat("1", 129))));
        Assert.Throws<MathParseException>(() => Calculator.Evaluate(new string('-', 500) + "1"));
        Assert.Throws<MathParseException>(() => Calculator.Evaluate(string.Join("^", Enumerable.Repeat("1", 500))));
        Assert.Throws<MathParseException>(() => Calculator.Evaluate("1" + new string('$', 129)));
    }

    [Test]
    public void ParserValidatesAndCopiesTokenSequence()
    {
        Assert.Throws<ArgumentNullException>(() => new Parser(null!));
        Assert.Throws<ArgumentException>(() => new Parser(new List<Token>()));
        Assert.Throws<ArgumentException>(() => new Parser(new List<Token> { new(TokenType.Number, "1", 0) }));
        Assert.Throws<ArgumentException>(() => new Parser(new List<Token> { new(TokenType.EOF, "", 0), new(TokenType.EOF, "", 0) }));
        Assert.Throws<ArgumentException>(() => new Parser(new List<Token> { new(TokenType.Number, "1", -1), new(TokenType.EOF, "", 1) }));
        var tokens = Lexer.BuildTokens("42");
        var parser = new Parser(tokens);
        tokens.Clear();
        Assert.That(parser.ParseExpression(), Is.EqualTo(new NumberExpr(42)));
        parser.ExpectEndOfInput();
        Assert.Throws<MathParseException>(() => parser.ParseExpression());
        Assert.Throws<ArgumentOutOfRangeException>(() => new Parser(Lexer.BuildTokens("1")).ParseExpression(-1));
    }

    [TestCase("de-DE")]
    [TestCase("en-US")]
    [TestCase("fr-FR")]
    public void NumericSyntaxIsCultureIndependentAndFormattingUsesCurrentCulture(string culture)
    {
        CultureInfo previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            Assert.That(Calculator.Evaluate(".5 + ,5"), Is.EqualTo(1));
            Assert.That(Calculator.Evaluate<string>("1234.5$"), Is.EqualTo(1234.5.ToString("N2") + "$"));
            Assert.That(Lexer.FloatingPointSeparator, Is.EqualTo(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator[0]));
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }

    [TestCase("sin(pi / 2)", 1)]
    [TestCase("cos(0)", 1)]
    [TestCase("tan(0)", 0)]
    [TestCase("log(e)", 1)]
    [TestCase("atan(1)", System.Math.PI / 4)]
    [TestCase("atan2(1;0)", System.Math.PI / 2)]
    [TestCase("π", System.Math.PI)]
    [TestCase("+2^2", 4)]
    [TestCase("2^-2", 0.25)]
    [TestCase("8/4/2", 1)]
    [TestCase("(2+3)$ * 2", 10)]
    public void MathematicalReferenceValues(string input, double expected) =>
        Assert.That(Calculator.Evaluate(input), Is.EqualTo(expected).Within(1e-12));

    [TestCase("0/0")]
    [TestCase("sqrt(-1)")]
    [TestCase("max(2;sqrt(-1))")]
    [TestCase("min(2;sqrt(-1))")]
    [TestCase("max(sqrt(-1);2)")]
    [TestCase("min(sqrt(-1);2)")]
    public void UndefinedNumericResultsPropagateNaN(string input) =>
        Assert.That(double.IsNaN(Calculator.Evaluate(input)), Is.True);

    [Test]
    public void ValueConversionsPreserveNullAndCompatibleObjects()
    {
        Assert.That(Value.Null.To<string?>(), Is.Null);
        Assert.That(Value.Null.To<int?>(), Is.Null);
        Assert.Throws<InvalidCastException>(() => Value.Null.To<int>());
        var amount = new CurrencyAmount(3.5, "$");
        Assert.That(new Value(amount).To<int?>(), Is.EqualTo(4));
        Assert.That(new Value(amount).To<CurrencyAmount>(), Is.EqualTo(amount));
        var reference = new List<int> { 1 };
        Assert.That(new Value(reference).To<IReadOnlyList<int>>(), Is.SameAs(reference));
        Assert.Throws<OverflowException>(() => new Value(double.PositiveInfinity).To<int>());
    }

    [Test]
    public void RegistrationsValidateArgumentsAndReturnSnapshots()
    {
        var evaluator = Evaluator.CreateCalculator();
        Assert.Throws<ArgumentException>(() => evaluator.Define(" ", _ => Value.Null));
        Assert.Throws<ArgumentNullException>(() => evaluator.Define("f", null!));
        Assert.Throws<ArgumentOutOfRangeException>(() => evaluator.Define("f", -1, _ => Value.Null));
        Assert.Throws<ArgumentOutOfRangeException>(() => evaluator.DefineVariadic("f", -1, _ => Value.Null));
        evaluator.GetVariables().Clear();
        evaluator.GetFunctions().Clear();
        Assert.That(ParserRuntime.RunOrThrow("sin(pi)", evaluator).To<double>(), Is.EqualTo(0).Within(1e-12));
        evaluator.DefineVariadic("sum", 0, values => new Value(values.Sum(value => value.To<double>())));
        Assert.That(ParserRuntime.RunOrThrow("sum(1;2;3)", evaluator).To<double>(), Is.EqualTo(6));
        Assert.That(ParserRuntime.RunOrThrow("sum()", evaluator).To<double>(), Is.Zero);
        Assert.That(ParserRuntime.TryRun("PI", evaluator, out _, out _), Is.False);
    }

    [Test]
    public void StatelessCallsAreIndependentUnderConcurrency()
    {
        Parallel.For(0, 500, i =>
        {
            Assert.That(Calculator.Evaluate($"{i} * 2 + ans"), Is.EqualTo(i * 2));
        });
    }

    [Test]
    public void FixedSeedMalformedInputFuzzingOnlyReturnsDocumentedErrors()
    {
        const string Alphabet = "0123456789+-*/%^();,. abc_\"$€√@";
        var random = new Random(71493);
        var evaluator = Evaluator.CreateCalculator();
        for (int sample = 0; sample < 5000; sample++)
        {
            string input = new(Enumerable.Range(0, random.Next(0, 96))
                .Select(_ => Alphabet[random.Next(Alphabet.Length)]).ToArray());
            bool success = ParserRuntime.TryRun(input, evaluator, out _, out string? error);
            Assert.That(success ? error == null : !string.IsNullOrEmpty(error), Is.True, input);
        }
    }

    [Test]
    public void FixedSeedGeneratedExpressionsMatchIndependentArithmetic()
    {
        var random = new Random(9137);
        for (int i = 0; i < 1000; i++)
        {
            int a = random.Next(-1000, 1000);
            int b = random.Next(-1000, 1000);
            int c = random.Next(1, 100);
            string expression = $"(({a})+({b}))*{c}";
            Assert.That(Calculator.Evaluate(expression), Is.EqualTo((a + b) * c));
        }
    }
}
