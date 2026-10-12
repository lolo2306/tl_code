namespace Tl.Compiler.Diagnostics;

public sealed class TlCompilerException : Exception
{
    public int? Line { get; }

    public int? Column { get; }

    public TlCompilerException(string message, int? line = null, int? column = null)
        : base(FormatMessage(message, line, column))
    {
        Line = line;
        Column = column;
    }

    private static string FormatMessage(string message, int? line, int? column)
    {
        if (line is not int l)
        {
            return message;
        }

        return column is int c ? $"{message} (ligne {l}, colonne {c})" : $"{message} (ligne {l})";
    }
}
