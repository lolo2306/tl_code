namespace Tl.Compiler.Parsing;

public enum TlType
{
    String,
    Int,
    Bool,
}

public abstract record Expression;

public sealed record StringLiteral(string Value) : Expression;

public sealed record IntLiteral(int Value) : Expression;

public sealed record BoolLiteral(bool Value) : Expression;

public sealed record VariableReference(string Name, TlType Type) : Expression;

public static class ExpressionExtensions
{
    public static TlType ResolveType(this Expression expression) => expression switch
    {
        StringLiteral => TlType.String,
        IntLiteral => TlType.Int,
        BoolLiteral => TlType.Bool,
        VariableReference variable => variable.Type,
        _ => throw new NotSupportedException($"Expression non supportée : {expression.GetType().Name}"),
    };
}

public abstract record Statement;

public sealed record PrintStatement(Expression Value) : Statement;

public sealed record VarDeclaration(string Name, Expression Value) : Statement;

public sealed record ProgramNode(IReadOnlyList<Statement> Statements);
