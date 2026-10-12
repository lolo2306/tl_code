using Tl.Compiler.Diagnostics;
using Tl.Compiler.Lexing;

namespace Tl.Compiler.Parsing;

public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private int _position;

    public Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
    }

    public ProgramNode Parse()
    {
        var statements = new List<Statement>();

        while (Current.Type != TokenType.Eof)
        {
            statements.Add(ParseStatement());
        }

        return new ProgramNode(statements);
    }

    private Statement ParseStatement()
    {
        Expect(TokenType.Print);
        var text = Expect(TokenType.String).Lexeme;
        Expect(TokenType.Semicolon);
        return new PrintStatement(text);
    }

    private Token Current => _tokens[_position];

    private Token Expect(TokenType type)
    {
        if (Current.Type != type)
        {
            throw new TlCompilerException(
                $"Attendu {type}, trouvé {Current.Type} ('{Current.Lexeme}')",
                Current.Line);
        }

        var token = Current;
        _position++;
        return token;
    }
}
