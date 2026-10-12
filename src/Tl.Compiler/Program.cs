using Tl.Compiler.Building;
using Tl.Compiler.CodeGen;
using Tl.Compiler.Diagnostics;
using Tl.Compiler.Lexing;
using Tl.Compiler.Parsing;

if (args.Length < 1)
{
    Console.Error.WriteLine("Usage : tlc <fichier.tl> [-o <sortie>]");
    return 1;
}

var inputPath = args[0];
var outputPath = GetOutputPath(args, inputPath);

try
{
    var source = File.ReadAllText(inputPath);
    var tokens = new Lexer(source).Tokenize();
    var program = new Parser(tokens).Parse();
    var csharpSource = CSharpCodeGenerator.Generate(program);

    NativeBuilder.Build(csharpSource, outputPath);

    Console.WriteLine($"Compilé : {outputPath}");
    return 0;
}
catch (TlCompilerException ex)
{
    Console.Error.WriteLine($"Erreur : {ex.Message}");
    return 1;
}

static string GetOutputPath(string[] args, string inputPath)
{
    for (var i = 1; i < args.Length - 1; i++)
    {
        if (args[i] == "-o")
        {
            return args[i + 1];
        }
    }

    return Path.GetFileNameWithoutExtension(inputPath);
}
