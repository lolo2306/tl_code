using System.Text;
using Tl.Compiler.Diagnostics;

namespace Tl.Compiler.Lexing;

public sealed class Lexer
{
    private readonly string _source;
    private int _position;
    private int _line = 1;

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
                tokens.Add(new Token(TokenType.Eof, string.Empty, _line));
                break;
            }

            tokens.Add(Current switch
            {
                '"' => ReadString(),
                ';' => ReadSingleChar(TokenType.Semicolon),
                var c when char.IsLetter(c) => ReadKeyword(),
                var c => throw new TlCompilerException($"Caractère inattendu '{c}'", _line),
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
        var lexeme = Advance().ToString();
        return new Token(type, lexeme, line);
    }

    private Token ReadString()
    {
        var startLine = _line;
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
                _ => throw new TlCompilerException($"Séquence d'échappement inconnue '\\{escaped}'", startLine),
            });
        }

        if (IsAtEnd)
        {
            throw new TlCompilerException("Chaîne de caractères non terminée", startLine);
        }

        Advance();
        return new Token(TokenType.String, text.ToString(), startLine);
    }

    private Token ReadKeyword()
    {
        var startLine = _line;
        var start = _position;

        while (!IsAtEnd && char.IsLetterOrDigit(Current))
        {
            Advance();
        }

        var text = _source[start.._position];

        return text switch
        {
            "print" => new Token(TokenType.Print, text, startLine),
            _ => throw new TlCompilerException($"Mot-clé inconnu '{text}'", startLine),
        };
    }
}
