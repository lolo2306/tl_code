using Tl.Compiler.Diagnostics;
using Tl.Compiler.Lexing;
using Tl.Compiler.Parsing;

namespace Tl.Compiler.Tests;

public class ParserTests
{
    [Fact]
    public void Parse_SinglePrintStatement_ProducesPrintStatement()
    {
        var tokens = new Lexer("print \"hello\";").Tokenize();

        var program = new Parser(tokens).Parse();

        var statement = Assert.Single(program.Statements);
        var printStatement = Assert.IsType<PrintStatement>(statement);
        Assert.Equal("hello", Assert.IsType<StringLiteral>(printStatement.Value).Value);
    }

    [Fact]
    public void Parse_MultiplePrintStatements_ProducesStatementsInOrder()
    {
        var tokens = new Lexer("print \"a\"; print \"b\";").Tokenize();

        var program = new Parser(tokens).Parse();

        Assert.Equal(2, program.Statements.Count);
        Assert.Equal("a", Assert.IsType<StringLiteral>(Assert.IsType<PrintStatement>(program.Statements[0]).Value).Value);
        Assert.Equal("b", Assert.IsType<StringLiteral>(Assert.IsType<PrintStatement>(program.Statements[1]).Value).Value);
    }

    [Fact]
    public void Parse_MissingSemicolon_Throws()
    {
        var tokens = new Lexer("print \"hello\"").Tokenize();

        Assert.Throws<TlCompilerException>(() => new Parser(tokens).Parse());
    }

    [Fact]
    public void Parse_IntAndBoolLiterals_ProducesTypedLiterals()
    {
        var tokens = new Lexer("print 42; print true; print false;").Tokenize();

        var program = new Parser(tokens).Parse();

        Assert.Equal(42, Assert.IsType<IntLiteral>(Assert.IsType<PrintStatement>(program.Statements[0]).Value).Value);
        Assert.True(Assert.IsType<BoolLiteral>(Assert.IsType<PrintStatement>(program.Statements[1]).Value).Value);
        Assert.False(Assert.IsType<BoolLiteral>(Assert.IsType<PrintStatement>(program.Statements[2]).Value).Value);
    }

    [Fact]
    public void Parse_VarDeclaration_RecordsNameAndValue()
    {
        var tokens = new Lexer("var x = \"hello\";").Tokenize();

        var program = new Parser(tokens).Parse();

        var declaration = Assert.IsType<VarDeclaration>(Assert.Single(program.Statements));
        Assert.Equal("x", declaration.Name);
        Assert.Equal("hello", Assert.IsType<StringLiteral>(declaration.Value).Value);
    }

    [Fact]
    public void Parse_PrintDeclaredVariable_ResolvesItsType()
    {
        var tokens = new Lexer("var x = 42; print x;").Tokenize();

        var program = new Parser(tokens).Parse();

        var printStatement = Assert.IsType<PrintStatement>(program.Statements[1]);
        var reference = Assert.IsType<VariableReference>(printStatement.Value);
        Assert.Equal("x", reference.Name);
        Assert.Equal(TlType.Int, reference.Type);
    }

    [Fact]
    public void Parse_UndeclaredVariable_Throws()
    {
        var tokens = new Lexer("print x;").Tokenize();

        Assert.Throws<TlCompilerException>(() => new Parser(tokens).Parse());
    }

    [Fact]
    public void Parse_DuplicateVarDeclaration_Throws()
    {
        var tokens = new Lexer("var x = 1; var x = 2;").Tokenize();

        Assert.Throws<TlCompilerException>(() => new Parser(tokens).Parse());
    }
}
