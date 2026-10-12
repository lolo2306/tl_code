using Tl.Compiler.Diagnostics;
using Tl.Compiler.Lexing;
using Tl.Compiler.Parsing;

namespace Tl.Compiler.Tests;

public class FunctionParsingTests
{
    private static ProgramNode Parse(string source) => new Parser(new Lexer(source).Tokenize()).Parse();

    [Fact]
    public void Parse_FunctionWithReturn_ProducesFunctionDeclaration()
    {
        var program = Parse("fn add(a: int, b: int) -> int { return a + b; }");

        var function = Assert.IsType<FunctionDeclaration>(Assert.Single(program.Statements));
        Assert.Equal("add", function.Name);
        Assert.Equal(2, function.Parameters.Count);
        Assert.Equal(TlType.Int, function.ReturnType);
    }

    [Fact]
    public void Parse_VoidFunctionWithoutReturnType_HasNullReturnType()
    {
        var program = Parse("fn greet() { print \"hi\"; }");

        var function = Assert.IsType<FunctionDeclaration>(Assert.Single(program.Statements));
        Assert.Null(function.ReturnType);
    }

    [Fact]
    public void Parse_CallExpression_ResolvesToFunctionReturnType()
    {
        var program = Parse("fn add(a: int, b: int) -> int { return a + b; } print add(1, 2);");

        var printStatement = Assert.IsType<PrintStatement>(program.Statements[1]);
        var call = Assert.IsType<CallExpression>(printStatement.Value);
        Assert.Equal("add", call.Name);
        Assert.Equal(2, call.Arguments.Count);
        Assert.Equal(TlType.Int, call.Type);
    }

    [Fact]
    public void Parse_CallAsStatement_ProducesExpressionStatement()
    {
        var program = Parse("fn greet() { print \"hi\"; } greet();");

        Assert.IsType<ExpressionStatement>(program.Statements[1]);
    }

    [Fact]
    public void Parse_DirectRecursion_Succeeds()
    {
        var program = Parse("fn fact(n: int) -> int { if (n == 0) { return 1; } return n * fact(n - 1); }");

        Assert.Single(program.Statements);
    }

    [Fact]
    public void Parse_FunctionCannotSeeOuterVariables_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("var x = 1; fn f() { print x; }"));
    }

    [Fact]
    public void Parse_FunctionDoesNotLeakParametersOutside_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("fn f(a: int) { print a; } print a;"));
    }

    [Fact]
    public void Parse_CallWithWrongArgumentCount_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("fn add(a: int, b: int) -> int { return a + b; } print add(1);"));
    }

    [Fact]
    public void Parse_CallWithWrongArgumentType_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("fn add(a: int, b: int) -> int { return a + b; } print add(1, true);"));
    }

    [Fact]
    public void Parse_CallToUndeclaredFunction_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("print missing(1);"));
    }

    [Fact]
    public void Parse_DuplicateFunctionDeclaration_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("fn f() { } fn f() { }"));
    }

    [Fact]
    public void Parse_DuplicateParameterName_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("fn f(a: int, a: int) { }"));
    }

    [Fact]
    public void Parse_ReturnOutsideFunction_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("return 1;"));
    }

    [Fact]
    public void Parse_ReturnWithValueInVoidFunction_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("fn f() { return 1; }"));
    }

    [Fact]
    public void Parse_ReturnWithoutValueInTypedFunction_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("fn f() -> int { return; }"));
    }

    [Fact]
    public void Parse_ReturnWithWrongType_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("fn f() -> int { return true; }"));
    }

    [Fact]
    public void Parse_PrintingVoidCall_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("fn f() { } print f();"));
    }

    [Fact]
    public void Parse_AssigningVoidCallToVariable_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("fn f() { } var x = f();"));
    }
}
