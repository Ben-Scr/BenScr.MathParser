using System;
using NUnit.Framework;
using BenScr.MathParser;

namespace BenScr.MathParser.Tests;

[TestFixture]
public class EdgeCaseTests
{
    private const double Tolerance = 1e-9;

    [TestCase("-2^2", ExpectedResult = 4.0)]
    [TestCase("2^-2", ExpectedResult = 0.25)]
    public double Power_EdgeCases(string expression)
    {
        return Calculator.Evaluate(expression);
    }

    [TestCase("√16", ExpectedResult = 4.0)]
    [TestCase("√16 + 9", ExpectedResult = 13.0)]
    public double SqrtOperator_BindsToSingleNumber(string expression)
    {
        return Calculator.Evaluate(expression);
    }

    [TestCase("5.5 % 2", ExpectedResult = 1.5)]
    [TestCase("-7 % 3", ExpectedResult = -1.0)]
    public double Modulo_EdgeCases(string expression)
    {
        return Calculator.Evaluate(expression);
    }

    [TestCase("2.5", ExpectedResult = 2)]
    [TestCase("3.5", ExpectedResult = 4)]
    [TestCase("4.5", ExpectedResult = 4)]
    public int EvaluateInt_UsesBankersRounding(string expression)
    {
        return Calculator.Evaluate<int>(expression);
    }

    [TestCase("1+2*3", ExpectedResult = 7.0)]
    [TestCase("   1   +   2   ", ExpectedResult = 3.0)]
    public double Whitespace_IsOptional(string expression)
    {
        return Calculator.Evaluate(expression);
    }

    [Test]
    public void MixedCurrency_AddsAmounts()
    {
        double result = Calculator.Evaluate("2$ + 3€");
        Assert.That(result, Is.EqualTo(5).Within(Tolerance));
    }

    [Test]
    public void MixedCurrency_KeepsFirstSymbolOnly()
    {
        string result = Calculator.Evaluate<string>("2$ + 3€");
        Assert.Multiple(() =>
        {
            Assert.That(result, Does.EndWith("$"));
            Assert.That(result, Does.Not.Contain("€"));
        });
    }

    [Test]
    public void StringLiteral_IsReturnedAsText()
    {
        string result = Calculator.Evaluate<string>("\"hello\"");
        Assert.That(result, Is.EqualTo("hello"));
    }

    [Test]
    public void Ans_ViaStatelessCalculator_IsAlwaysZero()
    {
        Calculator.Evaluate("5 + 5");
        double result = Calculator.Evaluate("ans");
        Assert.That(result, Is.EqualTo(0));
    }

    [TestCase("1.000,5")]
    [TestCase("max()")]
    public void UnsupportedInput_Throws(string expression)
    {
        Assert.That(() => Calculator.Evaluate(expression), Throws.Exception);
    }
}
