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

    [Fact]
    public void Generate_Assignment_EmitsPlainAssignment()
    {
        var program = new ProgramNode(new Statement[]
        {
            new VarDeclaration("x", new IntLiteral(1)),
            new AssignmentStatement("x", new IntLiteral(2)),
        });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("x = 2;", csharp);
    }

    [Fact]
    public void Generate_IfWithoutElse_EmitsIfBlockOnly()
    {
        var ifStatement = new IfStatement(
            new BoolLiteral(true),
            new BlockStatement(new Statement[] { new PrintStatement(new IntLiteral(1)) }),
            Else: null);
        var program = new ProgramNode(new Statement[] { ifStatement });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("if (true)", csharp);
        Assert.DoesNotContain("else", csharp);
    }

    [Fact]
    public void Generate_IfWithElse_EmitsBothBlocks()
    {
        var ifStatement = new IfStatement(
            new BoolLiteral(true),
            new BlockStatement(new Statement[] { new PrintStatement(new IntLiteral(1)) }),
            new BlockStatement(new Statement[] { new PrintStatement(new IntLiteral(2)) }));
        var program = new ProgramNode(new Statement[] { ifStatement });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("if (true)", csharp);
        Assert.Contains("else", csharp);
    }

    [Fact]
    public void Generate_While_EmitsWhileLoop()
    {
        var whileStatement = new WhileStatement(
            new BoolLiteral(true),
            new BlockStatement(new Statement[] { new PrintStatement(new IntLiteral(1)) }));
        var program = new ProgramNode(new Statement[] { whileStatement });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("while (true)", csharp);
    }

    [Fact]
    public void Generate_FunctionWithReturnType_EmitsStaticMethod()
    {
        var function = new FunctionDeclaration(
            "add",
            new[] { new Parameter("a", TlType.Int), new Parameter("b", TlType.Int) },
            TlType.Int,
            new BlockStatement(new Statement[]
            {
                new ReturnStatement(new BinaryExpression(new VariableReference("a", TlType.Int), BinaryOperator.Add, new VariableReference("b", TlType.Int), TlType.Int)),
            }));
        var program = new ProgramNode(new Statement[] { function });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("static int add(int a, int b)", csharp);
        Assert.Contains("return (a + b);", csharp);
    }

    [Fact]
    public void Generate_VoidFunction_EmitsVoidMethod()
    {
        var function = new FunctionDeclaration(
            "greet",
            Array.Empty<Parameter>(),
            null,
            new BlockStatement(new Statement[] { new PrintStatement(new StringLiteral("hi")) }));
        var program = new ProgramNode(new Statement[] { function });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("static void greet()", csharp);
    }

    [Fact]
    public void Generate_CallExpression_EmitsCallWithArguments()
    {
        var call = new CallExpression("add", new Expression[] { new IntLiteral(1), new IntLiteral(2) }, TlType.Int);
        var program = new ProgramNode(new Statement[] { new PrintStatement(call) });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("System.Console.WriteLine(add(1, 2));", csharp);
    }

    [Fact]
    public void Generate_CallStatement_EmitsStandaloneCall()
    {
        var call = new CallExpression("greet", Array.Empty<Expression>(), TlType.Void);
        var program = new ProgramNode(new Statement[] { new ExpressionStatement(call) });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.Contains("greet();", csharp);
    }

    [Fact]
    public void Generate_FunctionAppearsBeforeMain_RegardlessOfDeclarationOrder()
    {
        var function = new FunctionDeclaration("f", Array.Empty<Parameter>(), null, new BlockStatement(Array.Empty<Statement>()));
        var program = new ProgramNode(new Statement[]
        {
            new PrintStatement(new StringLiteral("before")),
            function,
        });

        var csharp = CSharpCodeGenerator.Generate(program);

        Assert.True(csharp.IndexOf("static void f()", StringComparison.Ordinal) < csharp.IndexOf("static void Main()", StringComparison.Ordinal));
    }
}
