# Migration from MathParserCS 1.0.7

The new package ID is BenScr.MathParser, version 1.0.0. This is a new package line with explicit breaking API changes.
Published MathParserCS versions and the original MathParser repository are preserved.

1. Remove the MathParserCS reference and add BenScr.MathParser.
2. Replace using BenScr.Math.Parser with using BenScr.MathParser.
3. Recompile all consumers: namespace and assembly identity changed.
4. Review exceptions, global lexer customization and console commands below.

The source master at 2d35f9ec1c92913884e0151a9467393a4d88f535 already used the new namespace.
It differs from the published 1.0.7 binary, whose namespace is BenScr.Math.Parser. Both inventories were checked.

| Previous API | Disposition |
| --- | --- |
| Calculator.Evaluate and Evaluate<T> | Retained under new namespace; calls are stateless. |
| Evaluator constructor, Define, SetVariable, GetVariables, GetFunctions | Retained; snapshots protect registration dictionaries. |
| Evaluator.Calculator | Retained alias. CreateCalculator from source master is the preferred factory. |
| DefineArithmetikOperations | Historical spelling retained as an alias for DefineArithmeticOperations. |
| DefineMathematicalFunctions | Retained with arity validation and explicit conversion errors. Resets ans and constants. |
| DefineDefaultFunctions | Removed: process termination, console clearing and exposing host paths belong to the application. Implement quit/exit/clear in the host command loop, not by evaluating untrusted expressions. credits/git labels can be registered through SetVariable by the host. |
| ParserRuntime.Run | Retained error-value form. RunOrThrow and TryRun from source master are retained. |
| Parser constructor and ParseExpression | Retained; tokens are copied and validated; bounds and positioned errors added. |
| ExpectEndOfInput | Retained from source master; runtime always rejects trailing tokens. |
| Lexer.BuildTokens, OppositeFloatingPointSeparator | Retained. Parsing always accepts dot/comma, formatting uses current culture. |
| Lexer.FloatingPointSeparator | Mutable static field replaced by a read-only current-culture property; assignments must be removed. |
| Lexer.Currencies | Mutable HashSet field replaced by immutable IReadOnlySet property for $ and €. Custom global currency mutation is no longer supported. |
| Token constructor, Type, Lexeme, Pos, ToString | Retained; fields remain readonly. |
| TokenType and historical Seperator | Numeric values and misspelled enum retained; Separator alias from source master retained. |
| Expr and NumberExpr, VarExpr, UnaryExpr, MoneyExpr, BinaryExpr, CallExpr, StringExpr | Retained, including record constructors, properties and generated equality/deconstruction members. |
| CurrencyAmount and Value, including Null, Object, To<T>, ToString | Retained; nullable currency conversion and compatible object conversion fixed. |

docs/LegacyPublicApi.txt inventories the actual published DLL. docs/PublicApi.txt inventories the reviewed new API.
docs/api-migration.json assigns every exported legacy member a disposition after namespace normalization.

Behavior preserved: .NET 9 compatibility, arithmetic precedence, right-associative power, unary-before-power,
both decimal separators, semicolon arguments, currency label propagation and current-culture formatting.
.NET 10 is added as an explicitly tested target.

Changes relative to source master:

- Inputs and trees are bounded. Excessive length/depth now raise MathParseException instead of risking stack exhaustion.
- Syntax failures expose exact source positions, including end-of-input; malformed token lists fail at construction.
- TryRun no longer catches unrelated callback/programming exceptions. Catch those in the host if appropriate.
- Empty runtime input is an expression error; Calculator's established empty/null argument checks remain.
- Built-in arity/type/range errors use ExpressionEvaluationException. NaN and infinity remain valid numerical outcomes.
- min/max now consistently propagate NaN; the former implementation ignored a later NaN.
- Unicode digits previously admitted by the lexer but not accepted by invariant numeric parsing are rejected at the original character.
- Value.To now handles nullable currency targets and compatible reference/value objects directly.

This is source migration, not a binary compatibility promise.
