using BenScr.MathParser;

namespace BenScr.MathParser.Tests
{
    [TestFixture]
    public class EvaluatorTests
    {
        private Evaluator _evaluator = null!;

        [SetUp]
        public void SetUp()
        {
            _evaluator = Evaluator.CreateCalculator();
        }

        [Test]
        public void RunOrThrow_EvaluatesExpression()
        {
            Value result = ParserRuntime.RunOrThrow("2 + 3", _evaluator);
            Assert.That(result.To<double>(), Is.EqualTo(5));
        }

        [Test]
        public void Ans_RemembersPreviousResult()
        {
            ParserRuntime.RunOrThrow("5 + 5", _evaluator);
            Value result = ParserRuntime.RunOrThrow("ans * 2", _evaluator);
            Assert.That(result.To<double>(), Is.EqualTo(20));
        }

        [Test]
        public void Define_CustomFunction_IsUsable()
        {
            _evaluator.Define("double", 1, args => new Value(args[0].To<double>() * 2));

            Value result = ParserRuntime.RunOrThrow("double(21)", _evaluator);
            Assert.That(result.To<double>(), Is.EqualTo(42));
        }

        [Test]
        public void SetVariable_CustomVariable_IsUsable()
        {
            _evaluator.SetVariable("x", new Value(10d));

            Value result = ParserRuntime.RunOrThrow("x * x", _evaluator);
            Assert.That(result.To<double>(), Is.EqualTo(100));
        }

        [Test]
        public void TryRun_ValidExpression_ReturnsTrueAndResult()
        {
            bool ok = ParserRuntime.TryRun("3 * 4", _evaluator, out Value value, out string? error);

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.True);
                Assert.That(error, Is.Null);
                Assert.That(value.To<double>(), Is.EqualTo(12));
            });
        }

        [Test]
        public void TryRun_InvalidExpression_ReturnsFalseAndError()
        {
            bool ok = ParserRuntime.TryRun("3 *", _evaluator, out _, out string? error);

            Assert.Multiple(() =>
            {
                Assert.That(ok, Is.False);
                Assert.That(error, Is.Not.Null.And.Not.Empty);
            });
        }

        [Test]
        public void Run_InvalidExpression_ReturnsErrorValue()
        {
            Value result = ParserRuntime.Run("1 +", _evaluator);
            Assert.That(result.ToString(), Does.StartWith("Error:"));
        }

        [Test]
        public void RunOrThrow_WrongArgumentCount_Throws()
        {
            Assert.That(() => ParserRuntime.RunOrThrow("pow(2)", _evaluator), Throws.Exception);
        }
    }
}
