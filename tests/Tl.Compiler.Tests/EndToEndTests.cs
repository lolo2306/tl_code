using System.Diagnostics;
using Tl.Compiler.Building;
using Tl.Compiler.CodeGen;
using Tl.Compiler.Lexing;
using Tl.Compiler.Parsing;

namespace Tl.Compiler.Tests;

public class EndToEndTests
{
    [Fact]
    public void CompileAndRun_HelloFixture_PrintsExpectedOutput()
    {
        var fixturePath = Path.Combine(AppContext.BaseDirectory, "fixtures", "hello.tl");
        var source = File.ReadAllText(fixturePath);

        var tokens = new Lexer(source).Tokenize();
        var program = new Parser(tokens).Parse();
        var csharp = CSharpCodeGenerator.Generate(program);

        var outputPath = Path.Combine(Path.GetTempPath(), "tlc-e2e-" + Guid.NewGuid());
        var executablePath = OperatingSystem.IsWindows() ? outputPath + ".exe" : outputPath;

        try
        {
            NativeBuilder.Build(csharp, outputPath);

            var startInfo = new ProcessStartInfo(executablePath)
            {
                RedirectStandardOutput = true,
            };

            using var process = Process.Start(startInfo)!;
            var stdout = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Assert.Equal(0, process.ExitCode);
            Assert.Equal("Hello, world!" + Environment.NewLine, stdout);
        }
        finally
        {
            if (File.Exists(executablePath))
            {
                File.Delete(executablePath);
            }
        }
    }
}
