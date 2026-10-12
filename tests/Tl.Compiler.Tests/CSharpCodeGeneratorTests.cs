using Tl.Compiler.CodeGen;
using Tl.Compiler.Parsing;

namespace Tl.Compiler.Tests;

public class CSharpCodeGeneratorTests
{
    [Fact]
    public void Generate_PrintStatement_EmitsConsoleWriteLineCall()
    {
        var program = new ProgramNode(new Statement[] { new PrintStatement("hello") });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("System.Console.WriteLine(\"hello\");", csharp);
    }

    [Fact]
    public void Generate_StringWithQuote_EscapesIt()
    {
        var program = new ProgramNode(new Statement[] { new PrintStatement("say \"hi\"") });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("System.Console.WriteLine(\"say \\\"hi\\\"\");", csharp);
    }
}
