using System.Text;
using Tl.Compiler.Diagnostics;

namespace Tl.Compiler.Lexing;

public sealed class Lexer
{
    private readonly string _source;
    private int _position;
    private int _line = 1;
    private int _column = 1;

    public Lexer(string source)
    {
        _source = source;
    }

    public List<Token> Tokenize()
    {
        var tokens = new List<Token>();

        while (true)
        {
            SkipWhitespace();

            if (IsAtEnd)
            {
                tokens.Add(new Token(TokenType.Eof, string.Empty, _line, _column));
                break;
            }

            tokens.Add(Current switch
            {
                '"' => ReadString(),
                ';' => ReadSingleChar(TokenType.Semicolon),
                '(' => ReadSingleChar(TokenType.LParen),
                ')' => ReadSingleChar(TokenType.RParen),
                '+' => ReadSingleChar(TokenType.Plus),
                '-' => ReadSingleChar(TokenType.Minus),
                '*' => ReadSingleChar(TokenType.Star),
                '/' => ReadSingleChar(TokenType.Slash),
                '%' => ReadSingleChar(TokenType.Percent),
                '=' => ReadOneOrTwoChar('=', TokenType.Equals, TokenType.EqualEqual),
                '!' => ReadOneOrTwoChar('=', TokenType.Bang, TokenType.NotEqual),
                '<' => ReadOneOrTwoChar('=', TokenType.Less, TokenType.LessEqual),
                '>' => ReadOneOrTwoChar('=', TokenType.Greater, TokenType.GreaterEqual),
                '&' => ReadRequiredTwoChar('&', TokenType.AndAnd),
                '|' => ReadRequiredTwoChar('|', TokenType.OrOr),
                var c when char.IsDigit(c) => ReadNumber(),
                var c when char.IsLetter(c) => ReadIdentifierOrKeyword(),
                var c => throw new TlCompilerException($"Caractère inattendu '{c}'", _line, _column),
            });
        }

        return tokens;
    }

    private bool IsAtEnd => _position >= _source.Length;

    private char Current => _source[_position];

    private char Advance()
    {
        var c = _source[_position];
        _position++;

        if (c == '\n')
        {
            _line++;
            _column = 1;
        }
        else
        {
            _column++;
        }

        return c;
    }

    private void SkipWhitespace()
    {
        while (!IsAtEnd && char.IsWhiteSpace(Current))
        {
            Advance();
        }
    }

    private Token ReadSingleChar(TokenType type)
    {
        var line = _line;
        var column = _column;
        var lexeme = Advance().ToString();
        return new Token(type, lexeme, line, column);
    }

    private Token ReadOneOrTwoChar(char second, TokenType single, TokenType double_)
    {
        var line = _line;
        var column = _column;
        var first = Advance();

        if (!IsAtEnd && Current == second)
        {
            Advance();
            return new Token(double_, $"{first}{second}", line, column);
        }

        return new Token(single, first.ToString(), line, column);
    }

    private Token ReadRequiredTwoChar(char expectedSecond, TokenType type)
    {
        var line = _line;
        var column = _column;
        var first = Advance();

        if (IsAtEnd || Current != expectedSecond)
        {
            throw new TlCompilerException($"Caractère inattendu '{first}'", line, column);
        }

        Advance();
        return new Token(type, $"{first}{expectedSecond}", line, column);
    }

    private Token ReadString()
    {
        var startLine = _line;
        var startColumn = _column;
        Advance();

        var text = new StringBuilder();

        while (!IsAtEnd && Current != '"')
        {
            var c = Advance();

            if (c != '\\')
            {
                text.Append(c);
                continue;
            }

            if (IsAtEnd)
            {
                break;
            }

            var escaped = Advance();
            text.Append(escaped switch
            {
                '"' => '"',
                '\\' => '\\',
                'n' => '\n',
                't' => '\t',
                _ => throw new TlCompilerException($"Séquence d'échappement inconnue '\\{escaped}'", startLine, startColumn),
            });
        }

        if (IsAtEnd)
        {
            throw new TlCompilerException("Chaîne de caractères non terminée", startLine, startColumn);
        }

        Advance();
        return new Token(TokenType.String, text.ToString(), startLine, startColumn);
    }

    private Token ReadNumber()
    {
        var startLine = _line;
        var startColumn = _column;
        var start = _position;

        while (!IsAtEnd && char.IsDigit(Current))
        {
            Advance();
        }

        return new Token(TokenType.Int, _source[start.._position], startLine, startColumn);
    }

    private Token ReadIdentifierOrKeyword()
    {
        var startLine = _line;
        var startColumn = _column;
        var start = _position;

        while (!IsAtEnd && char.IsLetterOrDigit(Current))
        {
            Advance();
        }

        var text = _source[start.._position];

        return text switch
        {
            "print" => new Token(TokenType.Print, text, startLine, startColumn),
            "var" => new Token(TokenType.Var, text, startLine, startColumn),
            "true" => new Token(TokenType.True, text, startLine, startColumn),
            "false" => new Token(TokenType.False, text, startLine, startColumn),
            _ => new Token(TokenType.Identifier, text, startLine, startColumn),
        };
    }
}
