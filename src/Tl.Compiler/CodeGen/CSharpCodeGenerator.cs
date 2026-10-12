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
            switch (statement)
            {
                case PrintStatement print:
                    source.AppendLine($"        System.Console.WriteLine({EscapeStringLiteral(print.Text)});");
                    break;
                default:
                    throw new NotSupportedException($"Instruction non supportée : {statement.GetType().Name}");
            }
        }

        source.AppendLine("    }");
        source.AppendLine("}");

        return source.ToString();
    }

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
