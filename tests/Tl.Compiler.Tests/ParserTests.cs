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
        Assert.Equal("hello", printStatement.Text);
    }

    [Fact]
    public void Parse_MultiplePrintStatements_ProducesStatementsInOrder()
    {
        var tokens = new Lexer("print \"a\"; print \"b\";").Tokenize();

        var program = new Parser(tokens).Parse();

        Assert.Equal(2, program.Statements.Count);
        Assert.Equal("a", Assert.IsType<PrintStatement>(program.Statements[0]).Text);
        Assert.Equal("b", Assert.IsType<PrintStatement>(program.Statements[1]).Text);
    }

    [Fact]
    public void Parse_MissingSemicolon_Throws()
    {
        var tokens = new Lexer("print \"hello\"").Tokenize();

        Assert.Throws<TlCompilerException>(() => new Parser(tokens).Parse());
    }
}
