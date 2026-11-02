# BenScr.MathParser

A small, extensible C# expression parser for .NET 9 and .NET 10, with no runtime package dependencies.

Created by Ben Schneider — https://github.com/Ben-Scr

```csharp
using BenScr.MathParser;

double result = Calculator.Evaluate("20 + 100 / 2 * 4^2"); // 820
double power = Calculator.Evaluate("2^3^2");              // 512
string money = Calculator.Evaluate<string>("2$ + 5$");   // "7.00$" in en-US
```

## Installation

[BenScr.MathParser 1.0.0 on NuGet](https://www.nuget.org/packages/BenScr.MathParser/1.0.0):

```sh
dotnet add package BenScr.MathParser --version 1.0.0
```

Review the [migration notes](https://github.com/Ben-Scr/BenScr.MathParser/blob/master/MIGRATION.md) before replacing MathParserCS.

## Sessions and custom functions

```csharp
Evaluator session = Evaluator.CreateCalculator();
session.SetVariable("x", new Value(21d));
session.Define("twice", 1, values => new Value(values[0].To<double>() * 2));

Value answer = ParserRuntime.RunOrThrow("twice(x)", session); // 42
Value next = ParserRuntime.RunOrThrow("ans + 1", session);     // 43

if (!ParserRuntime.TryRun("1 +", session, out Value value, out string? error))
{
    Console.WriteLine(error); // Unexpected token EOF at position 3.
}
```

A new Evaluator is empty. CreateCalculator registers arithmetic and mathematical functions.
Define replaces an existing function; DefineVariadic specifies a minimum argument count.
GetVariables and GetFunctions return shallow snapshots.
Callbacks are caller-owned, run synchronously, and may have side effects. They are not sandboxed.

## Expression contract

| Feature | Behavior |
| --- | --- |
| Numbers | ASCII decimal digits; dot or comma as decimal separator, independent of culture. No grouping separators or scientific notation. |
| Operators | +, -, *, /, %, ^, prefix + and -, prefix √, postfix $ and €. |
| Precedence, strongest first | Currency suffix; unary operators; power; multiplication/division/remainder; addition/subtraction. |
| Associativity | Power is right-associative; other binary operators are left-associative. |
| Historical unary behavior | -2^2 is 4; 2^-2 is 0.25. Use -(2^2) for -4. |
| Arguments | Semicolon-separated: pow(2;8). |
| Names | Case-sensitive; letters or underscore followed by letters, digits or underscores. |
| Constants | pi, π, e. ans starts at zero and changes only after successful session evaluation. |
| Functions | max, min (one or more arguments); pow, atan2 (two); clamp (three); sin, cos, tan, log, abs, atan, sqrt (one). |
| Mathematics | Double precision, radians, natural logarithm. IEEE 754 NaN/infinity propagate. min/max propagate NaN regardless of position. |
| Strings | Double-quoted literals; no escape syntax or string concatenation. |
| Currency | Approximate numeric values with display labels; no exchange-rate calculation or decimal financial arithmetic. Mixed labels retain the first label. |
| Conversion | Value.To follows Convert.ChangeType with current culture; integer conversions round ties to even. Already compatible objects are returned directly. |
| Formatting | Current culture. Currency uses N2 followed by the symbol, including culture-specific grouping. |

The immutable supported currency set and decimal syntax cannot be globally changed.
There is no implicit multiplication: write 2*pi, not 2pi.
String-to-number conversions inside built-in functions use the current culture.

## Errors and limits

Calculator rejects null with ArgumentNullException and empty/whitespace input with ArgumentException.
ParserRuntime treats empty input as a syntax error; null arguments remain programming errors.

MathParseException exposes Position, a zero-based UTF-16 source offset; EOF uses the input length.
ExpressionEvaluationException identifies unknown names, incorrect argument counts, or invalid built-in conversions/ranges.
TryRun returns false for these two exception types; Run returns an "Error: " string Value.
Unrelated custom callback exceptions propagate through all APIs. A callback can deliberately throw
ExpressionEvaluationException to report a recoverable expression error.

Input is limited to 16,384 UTF-16 code units. Parser recursion and syntax-tree depth are each limited to 128.
Limits are inclusive. A flat left-associative chain can reach the tree limit even without parentheses.
The limits bound parser-owned work; caller-provided callbacks and objects control their own resource use.
No cancellation, execution timeout, AOT, trimming or allocation-free guarantee is made.

Calculator and Lexer calls are independent and support concurrent use.
A shared Evaluator or Parser requires external synchronization; borrowed objects and callbacks keep their own contracts.
Syntax tree records remain public for compatibility. CallExpr.Args is a mutable, borrowed list.

## Build and verify

Requirements: SDK 10.0.303, .NET 9 and 10 runtimes, Git, and PowerShell 7. The SDK is pinned in global.json.
NUnit is retained from the source project; exact package versions and transitive locks are checked in.

```powershell
dotnet restore tests/BenScr.MathParser.Tests.csproj --locked-mode
dotnet test tests/BenScr.MathParser.Tests.csproj -c Release --no-restore
pwsh -File tools/Verify.ps1
```

The full verifier requires a clean checkout. It checks formatting, builds/tests both frameworks, compares the
reviewed public API baseline, runs the example, packs, checks archive metadata and PDB/source checksums,
and executes fresh consumers outside the repository using only the new package and an empty cache.
Artifact paths and hashes are in artifacts/latest-verification.txt and the selected release-manifest.json.
Use -ReleaseTag packages/BenScr.MathParser/1.0.0 for a tagged release candidate.

Windows and Ubuntu x64 passed [release CI](https://github.com/Ben-Scr/BenScr.MathParser/actions/runs/41497373390),
including 101 tests per framework and isolated package consumers.
The published 1.0.0 package also passed fresh consumer execution from nuget.org on .NET 9 and .NET 10;
public symbol downloads match the verified portable PDBs.
Other operating systems/architectures, trimming and native AOT are not release-tested promises.

## Release and provenance

See [release evidence](https://github.com/Ben-Scr/BenScr.MathParser/blob/master/RELEASE.md),
[migration notes](https://github.com/Ben-Scr/BenScr.MathParser/blob/master/MIGRATION.md),
[changelog](https://github.com/Ben-Scr/BenScr.MathParser/blob/master/CHANGELOG.md),
and [provenance](https://github.com/Ben-Scr/BenScr.MathParser/blob/master/docs/PROVENANCE.md).
The original author license is preserved in LICENSE.txt and embedded in the package.
The supplied BenScr logo is embedded byte-for-byte, with its original master in branding/.
