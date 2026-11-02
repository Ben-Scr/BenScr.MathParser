using System.Collections.Generic;

namespace BenScr.MathParser;

/// <summary>Base node of the public syntax tree. Nodes are data; call-argument lists remain caller-owned.</summary>
public abstract record Expr;
/// <summary>A parsed IEEE 754 number.</summary>
public record NumberExpr(double Value) : Expr;
/// <summary>A case-sensitive variable reference.</summary>
public record VarExpr(string Name) : Expr;
/// <summary>A prefix unary operator applied to its right operand.</summary>
public record UnaryExpr(TokenType Op, Expr Right) : Expr;
/// <summary>A currency label applied to the left expression.</summary>
public record MoneyExpr(Expr Left, string Lexeme) : Expr;
/// <summary>A binary operator with ordered operands.</summary>
public record BinaryExpr(TokenType Op, Expr Left, Expr Right) : Expr;
/// <summary>A named function call. The argument list is borrowed and mutable.</summary>
public record CallExpr(string CallName, List<Expr> Args) : Expr;
/// <summary>A string literal without quotes; escape sequences are not interpreted.</summary>
public record StringExpr(string Value) : Expr;
