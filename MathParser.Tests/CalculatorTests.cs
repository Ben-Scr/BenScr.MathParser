using BenScr.MathParser;

namespace BenScr.MathParser.Tests
{
    [TestFixture]
    public class CalculatorTests
    {
        private const double Tolerance = 1e-9;

        [Test]
        public void Evaluate_AddsTwoNumbers_ReturnsSum()
        {
            string expression = "1 + 5";
            double result = Calculator.Evaluate(expression);
            Assert.That(result, Is.EqualTo(6));
        }

        [TestCase("10 - 4", ExpectedResult = 6.0)]
        [TestCase("6 * 7", ExpectedResult = 42.0)]
        [TestCase("20 / 5", ExpectedResult = 4.0)]
        [TestCase("10 % 3", ExpectedResult = 1.0)]
        public double Evaluate_BasicArithmetic_ReturnsExpected(string expression)
        {
            return Calculator.Evaluate(expression);
        }

        [TestCase("1 + 2 * 3", ExpectedResult = 7.0)]
        [TestCase("3 + 5 % 2", ExpectedResult = 4.0)]
        [TestCase("(1 + 2) * 3", ExpectedResult = 9.0)]
        [TestCase("20 + 100 / 2 * 4^2", ExpectedResult = 820.0)]
        public double Evaluate_RespectsOperatorPrecedence(string expression)
        {
            return Calculator.Evaluate(expression);
        }

        [Test]
        public void Evaluate_Power_IsRightAssociative()
        {
            double result = Calculator.Evaluate("2^3^2");
            Assert.That(result, Is.EqualTo(512));
        }

        [Test]
        public void Evaluate_Subtraction_IsLeftAssociative()
        {
            double result = Calculator.Evaluate("10 - 3 - 2");
            Assert.That(result, Is.EqualTo(5));
        }

 
        [TestCase("pow(2;10)", ExpectedResult = 1024.0)]
        [TestCase("sqrt(81)", ExpectedResult = 9.0)]
        [TestCase("abs(-7)", ExpectedResult = 7.0)]
        [TestCase("max(3;9;1)", ExpectedResult = 9.0)]
        [TestCase("min(3;9;1)", ExpectedResult = 1.0)]
        [TestCase("clamp(15;0;10)", ExpectedResult = 10.0)]
        public double Evaluate_BuiltInFunctions_ReturnExpected(string expression)
        {
            return Calculator.Evaluate(expression);
        }

        [Test]
        public void Evaluate_Sin_OfZero_IsZero()
        {
            double result = Calculator.Evaluate("sin(0)");
            Assert.That(result, Is.EqualTo(0).Within(Tolerance));
        }

        [Test]
        public void Evaluate_PiConstant_MatchesMathPi()
        {
            double result = Calculator.Evaluate("pi");
            Assert.That(result, Is.EqualTo(System.Math.PI).Within(Tolerance));
        }

        [Test]
        public void Evaluate_TwoTimesPi_UsesMultiplication()
        {
            double result = Calculator.Evaluate("2 * pi");
            Assert.That(result, Is.EqualTo(2 * System.Math.PI).Within(Tolerance));
        }



        [Test]
        public void Evaluate_AcceptsBothDecimalSeparators()
        {
            double result = Calculator.Evaluate("1,5 + 1.5");
            Assert.That(result, Is.EqualTo(3).Within(Tolerance));
        }

        [Test]
        public void EvaluateGeneric_ToInt_RoundsResult()
        {
            int result = Calculator.Evaluate<int>("3.7");
            Assert.That(result, Is.EqualTo(4));
        }

        [Test]
        public void Evaluate_Currency_KeepsNumericValue()
        {
            double result = Calculator.Evaluate("2$ + 5$");
            Assert.That(result, Is.EqualTo(7).Within(Tolerance));
        }

        [Test]
        public void EvaluateGeneric_Currency_KeepsSymbolInText()
        {
            string result = Calculator.Evaluate<string>("2$ + 5$");

            Assert.Multiple(() =>
            {
                Assert.That(result, Does.Contain("7"));
                Assert.That(result, Does.EndWith("$"));
            });
        }


        [Test]
        public void Evaluate_EmptyInput_ThrowsArgumentException()
        {
            Assert.Throws<ArgumentException>(() => Calculator.Evaluate(""));
        }

        [Test]
        public void Evaluate_NullInput_ThrowsArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => Calculator.Evaluate(null!));
        }

        [TestCase("1 +")]
        [TestCase("(1 + 2")]
        [TestCase("foo(2)")]
        [TestCase("xyz")]
        public void Evaluate_InvalidInput_Throws(string expression)
        {
            Assert.That(() => Calculator.Evaluate(expression), Throws.Exception);
        }

        [Test]
        public void Evaluate_DivisionByZero_ReturnsInfinity()
        {
            double result = Calculator.Evaluate("1 / 0");
            Assert.That(double.IsPositiveInfinity(result), Is.True);
        }
    }
}
