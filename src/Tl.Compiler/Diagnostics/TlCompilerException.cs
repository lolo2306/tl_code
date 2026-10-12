namespace Tl.Compiler.Diagnostics;

public sealed class TlCompilerException : Exception
{
    public int? Line { get; }

    public TlCompilerException(string message, int? line = null)
        : base(line is int l ? $"{message} (ligne {l})" : message)
    {
        Line = line;
    }
}
