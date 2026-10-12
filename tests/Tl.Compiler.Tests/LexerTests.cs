using Tl.Compiler.Diagnostics;
using Tl.Compiler.Lexing;

namespace Tl.Compiler.Tests;

public class LexerTests
{
    [Fact]
    public void Tokenize_SimplePrintStatement_ProducesExpectedTokens()
    {
        var tokens = new Lexer("print \"Hello, world!\";").Tokenize();

        Assert.Collection(
            tokens,
            t => Assert.Equal(TokenType.Print, t.Type),
            t =>
            {
                Assert.Equal(TokenType.String, t.Type);
                Assert.Equal("Hello, world!", t.Lexeme);
            },
            t => Assert.Equal(TokenType.Semicolon, t.Type),
            t => Assert.Equal(TokenType.Eof, t.Type));
    }

    [Fact]
    public void Tokenize_EscapedQuoteInString_IsUnescaped()
    {
        var tokens = new Lexer("print \"say \\\"hi\\\"\";").Tokenize();

        Assert.Equal("say \"hi\"", tokens[1].Lexeme);
    }

    [Fact]
    public void Tokenize_UnterminatedString_Throws()
    {
        Assert.Throws<TlCompilerException>(() => new Lexer("print \"oops").Tokenize());
    }

    [Fact]
    public void Tokenize_VarDeclaration_ProducesExpectedTokens()
    {
        var tokens = new Lexer("var x = 42;").Tokenize();

        Assert.Collection(
            tokens,
            t => Assert.Equal(TokenType.Var, t.Type),
            t =>
            {
                Assert.Equal(TokenType.Identifier, t.Type);
                Assert.Equal("x", t.Lexeme);
            },
            t => Assert.Equal(TokenType.Equals, t.Type),
            t =>
            {
                Assert.Equal(TokenType.Int, t.Type);
                Assert.Equal("42", t.Lexeme);
            },
            t => Assert.Equal(TokenType.Semicolon, t.Type),
            t => Assert.Equal(TokenType.Eof, t.Type));
    }

    [Theory]
    [InlineData("true", TokenType.True)]
    [InlineData("false", TokenType.False)]
    public void Tokenize_BooleanKeywords_ProducesExpectedTokenType(string lexeme, TokenType expected)
    {
        var tokens = new Lexer(lexeme).Tokenize();

        Assert.Equal(expected, tokens[0].Type);
    }

    [Fact]
    public void Tokenize_UnexpectedCharacter_ReportsLineAndColumn()
    {
        var exception = Assert.Throws<TlCompilerException>(() => new Lexer("print \"ok\";\n  @").Tokenize());

        Assert.Equal(2, exception.Line);
        Assert.Equal(3, exception.Column);
    }
}
