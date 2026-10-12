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
        source.AppendLine("    static void Main()");
        source.AppendLine("    {");

        foreach (var statement in program.Statements)
        {
            source.AppendLine(EmitStatement(statement));
        }

        source.AppendLine("    }");
        source.AppendLine("}");

        return source.ToString();
    }

    private static string EmitStatement(Statement statement) => statement switch
    {
        PrintStatement print => $"        System.Console.WriteLine({EmitExpression(print.Value)});",
        VarDeclaration varDeclaration =>
            $"        {ToCSharpKeyword(varDeclaration.Value.ResolveType())} {varDeclaration.Name} = {EmitExpression(varDeclaration.Value)};",
        _ => throw new NotSupportedException($"Instruction non supportée : {statement.GetType().Name}"),
    };

    private static string EmitExpression(Expression expression) => expression switch
    {
        StringLiteral s => EscapeStringLiteral(s.Value),
        IntLiteral i => i.Value.ToString(CultureInfo.InvariantCulture),
        BoolLiteral b => b.Value ? "true" : "false",
        VariableReference v => v.Name,
        _ => throw new NotSupportedException($"Expression non supportée : {expression.GetType().Name}"),
    };

    private static string ToCSharpKeyword(TlType type) => type switch
    {
        TlType.String => "string",
        TlType.Int => "int",
        TlType.Bool => "bool",
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
