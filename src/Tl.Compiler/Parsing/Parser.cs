using System.Globalization;
using Tl.Compiler.Diagnostics;
using Tl.Compiler.Lexing;

namespace Tl.Compiler.Parsing;

public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private readonly Dictionary<string, TlType> _variables = new();
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

    private Statement ParseStatement() => Current.Type switch
    {
        TokenType.Print => ParsePrintStatement(),
        TokenType.Var => ParseVarDeclaration(),
        _ => throw new TlCompilerException(
            $"Instruction attendue, trouvé {Current.Type} ('{Current.Lexeme}')",
            Current.Line,
            Current.Column),
    };

    private Statement ParsePrintStatement()
    {
        Expect(TokenType.Print);
        var value = ParseValue();
        Expect(TokenType.Semicolon);
        return new PrintStatement(value);
    }

    private Statement ParseVarDeclaration()
    {
        Expect(TokenType.Var);
        var nameToken = Expect(TokenType.Identifier);

        if (_variables.ContainsKey(nameToken.Lexeme))
        {
            throw new TlCompilerException(
                $"Variable '{nameToken.Lexeme}' déjà déclarée",
                nameToken.Line,
                nameToken.Column);
        }

        Expect(TokenType.Equals);
        var value = ParseValue();
        Expect(TokenType.Semicolon);

        _variables[nameToken.Lexeme] = value.ResolveType();
        return new VarDeclaration(nameToken.Lexeme, value);
    }

    private Expression ParseValue()
    {
        var token = Current;

        switch (token.Type)
        {
            case TokenType.String:
                _position++;
                return new StringLiteral(token.Lexeme);
            case TokenType.Int:
                _position++;
                return new IntLiteral(int.Parse(token.Lexeme, CultureInfo.InvariantCulture));
            case TokenType.True:
                _position++;
                return new BoolLiteral(true);
            case TokenType.False:
                _position++;
                return new BoolLiteral(false);
            case TokenType.Identifier:
                _position++;
                if (!_variables.TryGetValue(token.Lexeme, out var type))
                {
                    throw new TlCompilerException(
                        $"Variable '{token.Lexeme}' non déclarée",
                        token.Line,
                        token.Column);
                }

                return new VariableReference(token.Lexeme, type);
            default:
                throw new TlCompilerException(
                    $"Valeur attendue, trouvé {token.Type} ('{token.Lexeme}')",
                    token.Line,
                    token.Column);
        }
    }

    private Token Current => _tokens[_position];

    private Token Expect(TokenType type)
    {
        if (Current.Type != type)
        {
            throw new TlCompilerException(
                $"Attendu {type}, trouvé {Current.Type} ('{Current.Lexeme}')",
                Current.Line,
                Current.Column);
        }

        var token = Current;
        _position++;
        return token;
    }
}
