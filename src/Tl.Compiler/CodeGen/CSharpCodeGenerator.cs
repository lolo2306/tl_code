using System.Globalization;
using System.Text;
using Tl.Compiler.Parsing;

namespace Tl.Compiler.CodeGen;

public static class CSharpCodeGenerator
{
    public static string Generate(ProgramNode program)
    {
        var source = new StringBuilder();
        source.AppendLine("class GeneratedProgram");
        source.AppendLine("{");

        foreach (var statement in program.Statements)
        {
            if (statement is FunctionDeclaration function)
            {
                EmitFunction(source, function);
            }
        }

        source.AppendLine("    static void Main()");
        source.AppendLine("    {");

        foreach (var statement in program.Statements)
        {
            if (statement is not FunctionDeclaration)
            {
                EmitStatement(source, statement, indent: 2);
            }
        }

        source.AppendLine("    }");
        source.AppendLine("}");

        return source.ToString();
    }

    private static void EmitFunction(StringBuilder source, FunctionDeclaration function)
    {
        var returnType = function.ReturnType is TlType rt ? ToCSharpKeyword(rt) : "void";
        var parameters = string.Join(", ", function.Parameters.Select(p => $"{ToCSharpKeyword(p.Type)} {p.Name}"));

        source.AppendLine($"    static {returnType} {function.Name}({parameters})");
        EmitBlock(source, function.Body, indent: 1);
    }

    private static void EmitStatement(StringBuilder source, Statement statement, int indent)
    {
        var pad = Indent(indent);

        switch (statement)
        {
            case PrintStatement print:
                source.AppendLine($"{pad}System.Console.WriteLine({EmitExpression(print.Value)});");
                break;

            case VarDeclaration varDeclaration:
                source.AppendLine(
                    $"{pad}{ToCSharpKeyword(varDeclaration.Value.ResolveType())} {varDeclaration.Name} = {EmitExpression(varDeclaration.Value)};");
                break;

            case AssignmentStatement assignment:
                source.AppendLine($"{pad}{assignment.Name} = {EmitExpression(assignment.Value)};");
                break;

            case IfStatement ifStatement:
                source.AppendLine($"{pad}if ({EmitExpression(ifStatement.Condition)})");
                EmitBlock(source, ifStatement.Then, indent);

                if (ifStatement.Else is not null)
                {
                    source.AppendLine($"{pad}else");
                    EmitBlock(source, ifStatement.Else, indent);
                }

                break;

            case WhileStatement whileStatement:
                source.AppendLine($"{pad}while ({EmitExpression(whileStatement.Condition)})");
                EmitBlock(source, whileStatement.Body, indent);
                break;

            case ReturnStatement returnStatement:
                source.AppendLine(returnStatement.Value is null
                    ? $"{pad}return;"
                    : $"{pad}return {EmitExpression(returnStatement.Value)};");
                break;

            case ExpressionStatement expressionStatement:
                source.AppendLine($"{pad}{EmitExpression(expressionStatement.Expression)};");
                break;

            default:
                throw new NotSupportedException($"Instruction non supportée : {statement.GetType().Name}");
        }
    }

    private static void EmitBlock(StringBuilder source, BlockStatement block, int indent)
    {
        var pad = Indent(indent);
        source.AppendLine($"{pad}{{");

        foreach (var statement in block.Statements)
        {
            EmitStatement(source, statement, indent + 1);
        }

        source.AppendLine($"{pad}}}");
    }

    private static string Indent(int level) => new(' ', level * 4);

    private static string EmitExpression(Expression expression) => expression switch
    {
        StringLiteral s => EscapeStringLiteral(s.Value),
        IntLiteral i => i.Value.ToString(CultureInfo.InvariantCulture),
        BoolLiteral b => b.Value ? "true" : "false",
        VariableReference v => v.Name,
        BinaryExpression bin => $"({EmitExpression(bin.Left)} {ToCSharpOperator(bin.Operator)} {EmitExpression(bin.Right)})",
        UnaryExpression un => $"({ToCSharpOperator(un.Operator)}{EmitExpression(un.Operand)})",
        CallExpression call => $"{call.Name}({string.Join(", ", call.Arguments.Select(EmitExpression))})",
        _ => throw new NotSupportedException($"Expression non supportée : {expression.GetType().Name}"),
    };

    private static string ToCSharpOperator(BinaryOperator op) => op switch
    {
        BinaryOperator.Add => "+",
        BinaryOperator.Subtract => "-",
        BinaryOperator.Multiply => "*",
        BinaryOperator.Divide => "/",
        BinaryOperator.Modulo => "%",
        BinaryOperator.Equal => "==",
        BinaryOperator.NotEqual => "!=",
        BinaryOperator.LessThan => "<",
        BinaryOperator.GreaterThan => ">",
        BinaryOperator.LessOrEqual => "<=",
        BinaryOperator.GreaterOrEqual => ">=",
        BinaryOperator.And => "&&",
        BinaryOperator.Or => "||",
        _ => throw new NotSupportedException($"Opérateur non supporté : {op}"),
    };

    private static string ToCSharpOperator(UnaryOperator op) => op switch
    {
        UnaryOperator.Negate => "-",
        UnaryOperator.Not => "!",
        _ => throw new NotSupportedException($"Opérateur non supporté : {op}"),
    };

    private static string ToCSharpKeyword(TlType type) => type switch
    {
        TlType.String => "string",
        TlType.Int => "int",
        TlType.Bool => "bool",
        TlType.Void => "void",
        _ => throw new NotSupportedException($"Type non supporté : {type}"),
    };

    private static string EscapeStringLiteral(string text)
    {
        var literal = new StringBuilder();
        literal.Append('"');

        foreach (var c in text)
        {
            literal.Append(c switch
            {
                '"' => "\\\"",
                '\\' => "\\\\",
                '\n' => "\\n",
                '\t' => "\\t",
                _ => c.ToString(),
            });
        }

        literal.Append('"');
        return literal.ToString();
    }
}
