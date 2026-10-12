namespace Tl.Compiler.Parsing;

public abstract record Statement;

public sealed record PrintStatement(string Text) : Statement;

public sealed record ProgramNode(IReadOnlyList<Statement> Statements);
