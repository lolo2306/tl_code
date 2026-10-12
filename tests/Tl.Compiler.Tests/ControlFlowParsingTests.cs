using Tl.Compiler.Diagnostics;
using Tl.Compiler.Lexing;
using Tl.Compiler.Parsing;

namespace Tl.Compiler.Tests;

public class ControlFlowParsingTests
{
    private static ProgramNode Parse(string source) => new Parser(new Lexer(source).Tokenize()).Parse();

    [Fact]
    public void Parse_IfWithoutElse_ProducesIfStatementWithNullElse()
    {
        var program = Parse("if (true) { print 1; }");

        var ifStatement = Assert.IsType<IfStatement>(Assert.Single(program.Statements));
        Assert.Null(ifStatement.Else);
        Assert.Single(ifStatement.Then.Statements);
    }

    [Fact]
    public void Parse_IfElse_ProducesBothBranches()
    {
        var program = Parse("if (true) { print 1; } else { print 2; }");

        var ifStatement = Assert.IsType<IfStatement>(Assert.Single(program.Statements));
        Assert.NotNull(ifStatement.Else);
        Assert.Single(ifStatement.Else!.Statements);
    }

    [Fact]
    public void Parse_ElseIfChain_NestsAsIfInsideElseBlock()
    {
        var program = Parse("if (false) { print 1; } else if (true) { print 2; } else { print 3; }");

        var outer = Assert.IsType<IfStatement>(Assert.Single(program.Statements));
        var nested = Assert.IsType<IfStatement>(Assert.Single(outer.Else!.Statements));
        Assert.NotNull(nested.Else);
    }

    [Fact]
    public void Parse_NonBoolCondition_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("if (1) { print 1; }"));
    }

    [Fact]
    public void Parse_While_ProducesWhileStatement()
    {
        var program = Parse("while (true) { print 1; }");

        var whileStatement = Assert.IsType<WhileStatement>(Assert.Single(program.Statements));
        Assert.Single(whileStatement.Body.Statements);
    }

    [Fact]
    public void Parse_WhileWithNonBoolCondition_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("while (1) { print 1; }"));
    }

    [Fact]
    public void Parse_Assignment_ToDeclaredVariable_Succeeds()
    {
        var program = Parse("var x = 1; x = 2;");

        var assignment = Assert.IsType<AssignmentStatement>(program.Statements[1]);
        Assert.Equal("x", assignment.Name);
        Assert.Equal(2, Assert.IsType<IntLiteral>(assignment.Value).Value);
    }

    [Fact]
    public void Parse_AssignmentToUndeclaredVariable_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("x = 2;"));
    }

    [Fact]
    public void Parse_AssignmentWithWrongType_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("var x = 1; x = true;"));
    }

    [Fact]
    public void Parse_VariableDeclaredInsideIfBlock_IsNotVisibleOutside()
    {
        Assert.Throws<TlCompilerException>(() => Parse("if (true) { var x = 1; } print x;"));
    }

    [Fact]
    public void Parse_VariableDeclaredInOuterScope_IsVisibleAndAssignableInsideWhileBody()
    {
        var program = Parse("var i = 0; while (i < 3) { i = i + 1; }");

        var whileStatement = Assert.IsType<WhileStatement>(program.Statements[1]);
        Assert.IsType<AssignmentStatement>(Assert.Single(whileStatement.Body.Statements));
    }

    [Fact]
    public void Parse_SameNameInSiblingBlocks_DoesNotConflict()
    {
        var program = Parse("if (true) { var x = 1; } if (true) { var x = 2; }");

        Assert.Equal(2, program.Statements.Count);
    }

    [Fact]
    public void Parse_RedeclaringOuterVariableInNestedBlock_Throws()
    {
        Assert.Throws<TlCompilerException>(() => Parse("var x = 1; if (true) { var x = 2; }"));
    }
}
