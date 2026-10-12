using Tl.Compiler.Diagnostics;
using Tl.Compiler.Lexing;
using Tl.Compiler.Parsing;

namespace Tl.Compiler.Tests;

public class ExpressionParsingTests
{
    private static Expression ParseSingleValue(string source)
    {
        var tokens = new Lexer($"print {source};").Tokenize();
        var program = new Parser(tokens).Parse();
        return Assert.IsType<PrintStatement>(Assert.Single(program.Statements)).Value;
    }

    [Fact]
    public void Parse_MultiplicationBeforeAddition_RespectsPrecedence()
    {
        var expression = Assert.IsType<BinaryExpression>(ParseSingleValue("1 + 2 * 3"));

        Assert.Equal(BinaryOperator.Add, expression.Operator);
        Assert.IsType<IntLiteral>(expression.Left);
        var right = Assert.IsType<BinaryExpression>(expression.Right);
        Assert.Equal(BinaryOperator.Multiply, right.Operator);
    }

    [Fact]
    public void Parse_Parentheses_OverridePrecedence()
    {
        var expression = Assert.IsType<BinaryExpression>(ParseSingleValue("(1 + 2) * 3"));

        Assert.Equal(BinaryOperator.Multiply, expression.Operator);
        var left = Assert.IsType<BinaryExpression>(expression.Left);
        Assert.Equal(BinaryOperator.Add, left.Operator);
    }

    [Fact]
    public void Parse_ArithmeticExpression_ResolvesToInt()
    {
        var expression = ParseSingleValue("1 + 2 * 3");

        Assert.Equal(TlType.Int, expression.ResolveType());
    }

    [Fact]
    public void Parse_ComparisonExpression_ResolvesToBool()
    {
        var expression = ParseSingleValue("1 < 2");

        Assert.Equal(TlType.Bool, expression.ResolveType());
    }

    [Fact]
    public void Parse_LogicalExpression_ResolvesToBool()
    {
        var expression = ParseSingleValue("true && (1 < 2)");

        Assert.Equal(TlType.Bool, expression.ResolveType());
    }

    [Fact]
    public void Parse_UnaryMinus_NegatesInt()
    {
        var expression = Assert.IsType<UnaryExpression>(ParseSingleValue("-42"));

        Assert.Equal(UnaryOperator.Negate, expression.Operator);
        Assert.Equal(TlType.Int, expression.Type);
    }

    [Fact]
    public void Parse_UnaryNot_NegatesBool()
    {
        var expression = Assert.IsType<UnaryExpression>(ParseSingleValue("!true"));

        Assert.Equal(UnaryOperator.Not, expression.Operator);
        Assert.Equal(TlType.Bool, expression.Type);
    }

    [Fact]
    public void Parse_AddingStringAndInt_Throws()
    {
        Assert.Throws<TlCompilerException>(() => ParseSingleValue("\"a\" + 1"));
    }

    [Fact]
    public void Parse_AndOnIntegers_Throws()
    {
        Assert.Throws<TlCompilerException>(() => ParseSingleValue("1 && 2"));
    }

    [Fact]
    public void Parse_EqualityBetweenDifferentTypes_Throws()
    {
        Assert.Throws<TlCompilerException>(() => ParseSingleValue("1 == true"));
    }

    [Fact]
    public void Parse_UnaryMinusOnBool_Throws()
    {
        Assert.Throws<TlCompilerException>(() => ParseSingleValue("-true"));
    }
}
