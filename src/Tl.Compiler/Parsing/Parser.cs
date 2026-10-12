using System.Globalization;
using Tl.Compiler.Diagnostics;
using Tl.Compiler.Lexing;

namespace Tl.Compiler.Parsing;

public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private readonly List<Dictionary<string, TlType>> _scopes = new() { new() };
    private int _position;

    public Parser(IReadOnlyList<Token> tokens)
    {
        _tokens = tokens;
    }

    public ProgramNode Parse()
    {
        var statements = new List<Statement>();

        while (Current.Type != TokenType.Eof)
        {
            statements.Add(ParseStatement());
        }

        return new ProgramNode(statements);
    }

    private Statement ParseStatement() => Current.Type switch
    {
        TokenType.Print => ParsePrintStatement(),
        TokenType.Var => ParseVarDeclaration(),
        TokenType.If => ParseIfStatement(),
        TokenType.While => ParseWhileStatement(),
        TokenType.Identifier => ParseAssignment(),
        _ => throw new TlCompilerException(
            $"Instruction attendue, trouvé {Current.Type} ('{Current.Lexeme}')",
            Current.Line,
            Current.Column),
    };

    private Statement ParsePrintStatement()
    {
        Expect(TokenType.Print);
        var value = ParseExpression();
        Expect(TokenType.Semicolon);
        return new PrintStatement(value);
    }

    private Statement ParseVarDeclaration()
    {
        Expect(TokenType.Var);
        var nameToken = Expect(TokenType.Identifier);

        if (IsDeclaredAnywhere(nameToken.Lexeme))
        {
            throw new TlCompilerException(
                $"Variable '{nameToken.Lexeme}' déjà déclarée",
                nameToken.Line,
                nameToken.Column);
        }

        Expect(TokenType.Equals);
        var value = ParseExpression();
        Expect(TokenType.Semicolon);

        DeclareVariable(nameToken.Lexeme, value.ResolveType());
        return new VarDeclaration(nameToken.Lexeme, value);
    }

    private Statement ParseAssignment()
    {
        var nameToken = Expect(TokenType.Identifier);

        if (!TryResolveVariable(nameToken.Lexeme, out var declaredType))
        {
            throw new TlCompilerException(
                $"Variable '{nameToken.Lexeme}' non déclarée",
                nameToken.Line,
                nameToken.Column);
        }

        Expect(TokenType.Equals);
        var value = ParseExpression();
        Expect(TokenType.Semicolon);

        if (value.ResolveType() != declaredType)
        {
            throw new TlCompilerException(
                $"Impossible d'assigner {Describe(value.ResolveType())} à '{nameToken.Lexeme}' ({Describe(declaredType)})",
                nameToken.Line,
                nameToken.Column);
        }

        return new AssignmentStatement(nameToken.Lexeme, value);
    }

    private Statement ParseIfStatement()
    {
        var keywordToken = Expect(TokenType.If);
        Expect(TokenType.LParen);
        var condition = ParseExpression();
        Expect(TokenType.RParen);
        RequireConditionType(condition, keywordToken);

        var thenBlock = ParseBlock();
        BlockStatement? elseBlock = null;

        if (Current.Type == TokenType.Else)
        {
            _position++;
            elseBlock = Current.Type == TokenType.If
                ? new BlockStatement(new[] { ParseIfStatement() })
                : ParseBlock();
        }

        return new IfStatement(condition, thenBlock, elseBlock);
    }

    private Statement ParseWhileStatement()
    {
        var keywordToken = Expect(TokenType.While);
        Expect(TokenType.LParen);
        var condition = ParseExpression();
        Expect(TokenType.RParen);
        RequireConditionType(condition, keywordToken);

        var body = ParseBlock();
        return new WhileStatement(condition, body);
    }

    private BlockStatement ParseBlock()
    {
        Expect(TokenType.LBrace);
        EnterScope();

        var statements = new List<Statement>();

        while (Current.Type != TokenType.RBrace)
        {
            if (Current.Type == TokenType.Eof)
            {
                throw new TlCompilerException("Bloc non terminé, '}' attendu", Current.Line, Current.Column);
            }

            statements.Add(ParseStatement());
        }

        ExitScope();
        Expect(TokenType.RBrace);
        return new BlockStatement(statements);
    }

    // programme de précédence (du plus faible au plus fort) :
    // || -> && -> ==/!= -> </>/<=/>= -> +/- -> * / % -> unaire (-, !) -> primaire
    private Expression ParseExpression() => ParseOr();

    private Expression ParseOr()
    {
        var left = ParseAnd();

        while (Current.Type == TokenType.OrOr)
        {
            var opToken = Current;
            _position++;
            left = MakeBinary(left, BinaryOperator.Or, ParseAnd(), opToken);
        }

        return left;
    }

    private Expression ParseAnd()
    {
        var left = ParseEquality();

        while (Current.Type == TokenType.AndAnd)
        {
            var opToken = Current;
            _position++;
            left = MakeBinary(left, BinaryOperator.And, ParseEquality(), opToken);
        }

        return left;
    }

    private Expression ParseEquality()
    {
        var left = ParseRelational();

        while (Current.Type is TokenType.EqualEqual or TokenType.NotEqual)
        {
            var opToken = Current;
            _position++;
            var op = opToken.Type == TokenType.EqualEqual ? BinaryOperator.Equal : BinaryOperator.NotEqual;
            left = MakeBinary(left, op, ParseRelational(), opToken);
        }

        return left;
    }

    private Expression ParseRelational()
    {
        var left = ParseAdditive();

        while (Current.Type is TokenType.Less or TokenType.Greater or TokenType.LessEqual or TokenType.GreaterEqual)
        {
            var opToken = Current;
            _position++;
            var op = opToken.Type switch
            {
                TokenType.Less => BinaryOperator.LessThan,
                TokenType.Greater => BinaryOperator.GreaterThan,
                TokenType.LessEqual => BinaryOperator.LessOrEqual,
                TokenType.GreaterEqual => BinaryOperator.GreaterOrEqual,
                _ => throw new NotSupportedException(),
            };
            left = MakeBinary(left, op, ParseAdditive(), opToken);
        }

        return left;
    }

    private Expression ParseAdditive()
    {
        var left = ParseMultiplicative();

        while (Current.Type is TokenType.Plus or TokenType.Minus)
        {
            var opToken = Current;
            _position++;
            var op = opToken.Type == TokenType.Plus ? BinaryOperator.Add : BinaryOperator.Subtract;
            left = MakeBinary(left, op, ParseMultiplicative(), opToken);
        }

        return left;
    }

    private Expression ParseMultiplicative()
    {
        var left = ParseUnary();

        while (Current.Type is TokenType.Star or TokenType.Slash or TokenType.Percent)
        {
            var opToken = Current;
            _position++;
            var op = opToken.Type switch
            {
                TokenType.Star => BinaryOperator.Multiply,
                TokenType.Slash => BinaryOperator.Divide,
                TokenType.Percent => BinaryOperator.Modulo,
                _ => throw new NotSupportedException(),
            };
            left = MakeBinary(left, op, ParseUnary(), opToken);
        }

        return left;
    }

    private Expression ParseUnary()
    {
        if (Current.Type is TokenType.Minus or TokenType.Bang)
        {
            var opToken = Current;
            _position++;
            var operand = ParseUnary();
            var op = opToken.Type == TokenType.Minus ? UnaryOperator.Negate : UnaryOperator.Not;
            var expectedType = op == UnaryOperator.Negate ? TlType.Int : TlType.Bool;

            RequireType(operand.ResolveType(), expectedType, opToken);
            return new UnaryExpression(op, operand, expectedType);
        }

        return ParsePrimary();
    }

    private Expression ParsePrimary()
    {
        var token = Current;

        switch (token.Type)
        {
            case TokenType.String:
                _position++;
                return new StringLiteral(token.Lexeme);
            case TokenType.Int:
                _position++;
                return new IntLiteral(int.Parse(token.Lexeme, CultureInfo.InvariantCulture));
            case TokenType.True:
                _position++;
                return new BoolLiteral(true);
            case TokenType.False:
                _position++;
                return new BoolLiteral(false);
            case TokenType.Identifier:
                _position++;
                if (!TryResolveVariable(token.Lexeme, out var type))
                {
                    throw new TlCompilerException($"Variable '{token.Lexeme}' non déclarée", token.Line, token.Column);
                }

                return new VariableReference(token.Lexeme, type);
            case TokenType.LParen:
                _position++;
                var inner = ParseExpression();
                Expect(TokenType.RParen);
                return inner;
            default:
                throw new TlCompilerException(
                    $"Valeur attendue, trouvé {token.Type} ('{token.Lexeme}')",
                    token.Line,
                    token.Column);
        }
    }

    private static Expression MakeBinary(Expression left, BinaryOperator op, Expression right, Token opToken)
    {
        var leftType = left.ResolveType();
        var rightType = right.ResolveType();
        TlType resultType;

        switch (op)
        {
            case BinaryOperator.Add:
            case BinaryOperator.Subtract:
            case BinaryOperator.Multiply:
            case BinaryOperator.Divide:
            case BinaryOperator.Modulo:
                RequireType(leftType, TlType.Int, opToken);
                RequireType(rightType, TlType.Int, opToken);
                resultType = TlType.Int;
                break;

            case BinaryOperator.LessThan:
            case BinaryOperator.GreaterThan:
            case BinaryOperator.LessOrEqual:
            case BinaryOperator.GreaterOrEqual:
                RequireType(leftType, TlType.Int, opToken);
                RequireType(rightType, TlType.Int, opToken);
                resultType = TlType.Bool;
                break;

            case BinaryOperator.Equal:
            case BinaryOperator.NotEqual:
                if (leftType != rightType)
                {
                    throw new TlCompilerException(
                        $"Comparaison entre types différents ({Describe(leftType)} et {Describe(rightType)})",
                        opToken.Line,
                        opToken.Column);
                }

                resultType = TlType.Bool;
                break;

            case BinaryOperator.And:
            case BinaryOperator.Or:
                RequireType(leftType, TlType.Bool, opToken);
                RequireType(rightType, TlType.Bool, opToken);
                resultType = TlType.Bool;
                break;

            default:
                throw new NotSupportedException($"Opérateur non supporté : {op}");
        }

        return new BinaryExpression(left, op, right, resultType);
    }

    private static void RequireType(TlType actual, TlType expected, Token opToken)
    {
        if (actual != expected)
        {
            throw new TlCompilerException(
                $"Opérateur '{opToken.Lexeme}' attend {Describe(expected)}, trouvé {Describe(actual)}",
                opToken.Line,
                opToken.Column);
        }
    }

    private static void RequireConditionType(Expression condition, Token keywordToken)
    {
        if (condition.ResolveType() != TlType.Bool)
        {
            throw new TlCompilerException(
                $"La condition doit être un booléen, trouvé {Describe(condition.ResolveType())}",
                keywordToken.Line,
                keywordToken.Column);
        }
    }

    private static string Describe(TlType type) => type switch
    {
        TlType.String => "une chaîne",
        TlType.Int => "un entier",
        TlType.Bool => "un booléen",
        _ => type.ToString(),
    };

    private void EnterScope() => _scopes.Add(new Dictionary<string, TlType>());

    private void ExitScope() => _scopes.RemoveAt(_scopes.Count - 1);

    private void DeclareVariable(string name, TlType type) => _scopes[^1][name] = type;

    private bool IsDeclaredAnywhere(string name) => _scopes.Any(scope => scope.ContainsKey(name));

    private bool TryResolveVariable(string name, out TlType type)
    {
        for (var i = _scopes.Count - 1; i >= 0; i--)
        {
            if (_scopes[i].TryGetValue(name, out type))
            {
                return true;
            }
        }

        type = default;
        return false;
    }

    private Token Current => _tokens[_position];

    private Token Expect(TokenType type)
    {
        if (Current.Type != type)
        {
            throw new TlCompilerException(
                $"Attendu {type}, trouvé {Current.Type} ('{Current.Lexeme}')",
                Current.Line,
                Current.Column);
        }

        var token = Current;
        _position++;
        return token;
    }
}
