namespace Tl.Compiler.Lexing;

public readonly record struct Token(TokenType Type, string Lexeme, int Line);
