using Tl.Compiler.CodeGen;
using Tl.Compiler.Parsing;

namespace Tl.Compiler.Tests;

public class CSharpCodeGeneratorTests
{
    [Fact]
    public void Generate_PrintStatement_EmitsConsoleWriteLineCall()
    {
        var program = new ProgramNode(new Statement[] { new PrintStatement(new StringLiteral("hello")) });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("System.Console.WriteLine(\"hello\");", csharp);
    }

    [Fact]
    public void Generate_StringWithQuote_EscapesIt()
    {
        var program = new ProgramNode(new Statement[] { new PrintStatement(new StringLiteral("say \"hi\"")) });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("System.Console.WriteLine(\"say \\\"hi\\\"\");", csharp);
    }

    [Fact]
    public void Generate_VarDeclaration_EmitsTypedLocal()
    {
        var program = new ProgramNode(new Statement[] { new VarDeclaration("x", new IntLiteral(42)) });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("int x = 42;", csharp);
    }

    [Fact]
    public void Generate_PrintVariableReference_EmitsItsName()
    {
        var program = new ProgramNode(new Statement[]
        {
            new VarDeclaration("flag", new BoolLiteral(true)),
            new PrintStatement(new VariableReference("flag", TlType.Bool)),
        });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("bool flag = true;", csharp);
        Assert.Contains("System.Console.WriteLine(flag);", csharp);
    }

    [Fact]
    public void Generate_BinaryExpression_EmitsParenthesizedOperator()
    {
        var expression = new BinaryExpression(new IntLiteral(1), BinaryOperator.Add, new IntLiteral(2), TlType.Int);
        var program = new ProgramNode(new Statement[] { new PrintStatement(expression) });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("System.Console.WriteLine((1 + 2));", csharp);
    }

    [Fact]
    public void Generate_UnaryExpression_EmitsParenthesizedOperator()
    {
        var expression = new UnaryExpression(UnaryOperator.Not, new BoolLiteral(true), TlType.Bool);
        var program = new ProgramNode(new Statement[] { new PrintStatement(expression) });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("System.Console.WriteLine((!true));", csharp);
    }
}
