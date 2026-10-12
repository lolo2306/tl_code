namespace Tl.Compiler.Parsing;

public enum TlType
{
    String,
    Int,
    Bool,
}

public enum BinaryOperator
{
    Add,
    Subtract,
    Multiply,
    Divide,
    Modulo,
    Equal,
    NotEqual,
    LessThan,
    GreaterThan,
    LessOrEqual,
    GreaterOrEqual,
    And,
    Or,
}

public enum UnaryOperator
{
    Negate,
    Not,
}

public abstract record Expression;

public sealed record StringLiteral(string Value) : Expression;

public sealed record IntLiteral(int Value) : Expression;

public sealed record BoolLiteral(bool Value) : Expression;

public sealed record VariableReference(string Name, TlType Type) : Expression;

public sealed record BinaryExpression(Expression Left, BinaryOperator Operator, Expression Right, TlType Type) : Expression;

public sealed record UnaryExpression(UnaryOperator Operator, Expression Operand, TlType Type) : Expression;

public static class ExpressionExtensions
{
    public static TlType ResolveType(this Expression expression) => expression switch
    {
        StringLiteral => TlType.String,
        IntLiteral => TlType.Int,
        BoolLiteral => TlType.Bool,
        VariableReference variable => variable.Type,
        BinaryExpression binary => binary.Type,
        UnaryExpression unary => unary.Type,
        _ => throw new NotSupportedException($"Expression non supportée : {expression.GetType().Name}"),
    };
}

public abstract record Statement;

public sealed record PrintStatement(Expression Value) : Statement;

public sealed record VarDeclaration(string Name, Expression Value) : Statement;

public sealed record AssignmentStatement(string Name, Expression Value) : Statement;

public sealed record BlockStatement(IReadOnlyList<Statement> Statements) : Statement;

public sealed record IfStatement(Expression Condition, BlockStatement Then, BlockStatement? Else) : Statement;

public sealed record WhileStatement(Expression Condition, BlockStatement Body) : Statement;

public sealed record ProgramNode(IReadOnlyList<Statement> Statements);
