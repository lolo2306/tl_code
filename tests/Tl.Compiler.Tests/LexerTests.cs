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
    public void Tokenize_UnknownKeyword_Throws()
    {
        Assert.Throws<TlCompilerException>(() => new Lexer("println \"hi\";").Tokenize());
    }
}
