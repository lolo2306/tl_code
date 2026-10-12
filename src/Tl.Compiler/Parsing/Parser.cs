using System.Globalization;
using Tl.Compiler.Diagnostics;
using Tl.Compiler.Lexing;

namespace Tl.Compiler.Parsing;

public sealed class Parser
{
    private readonly IReadOnlyList<Token> _tokens;
    private readonly Dictionary<string, FunctionSignature> _functions = new();
    private List<Dictionary<string, TlType>> _scopes = new() { new() };
    private bool _insideFunction;
    private TlType? _currentFunctionReturnType;
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
            statements.Add(Current.Type == TokenType.Fn ? ParseFunctionDeclaration() : ParseStatement());
        }

        return new ProgramNode(statements);
    }

    private Statement ParseStatement() => Current.Type switch
    {
        TokenType.Print => ParsePrintStatement(),
        TokenType.Var => ParseVarDeclaration(),
        TokenType.If => ParseIfStatement(),
        TokenType.While => ParseWhileStatement(),
        TokenType.Return => ParseReturnStatement(),
        TokenType.Identifier when PeekNext().Type == TokenType.LParen => ParseCallStatement(),
        TokenType.Identifier => ParseAssignment(),
        _ => throw new TlCompilerException(
            $"Instruction attendue, trouvé {Current.Type} ('{Current.Lexeme}')",
            Current.Line,
            Current.Column),
    };

    private Statement ParsePrintStatement()
    {
        var keywordToken = Expect(TokenType.Print);
        var value = ParseExpression();
        Expect(TokenType.Semicolon);

        if (value.ResolveType() == TlType.Void)
        {
            throw new TlCompilerException(
                "Impossible d'afficher le résultat d'un appel qui ne retourne rien (void)",
                keywordToken.Line,
                keywordToken.Column);
        }

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

        if (value.ResolveType() == TlType.Void)
        {
            throw new TlCompilerException(
                $"Impossible d'assigner à '{nameToken.Lexeme}' le résultat d'un appel qui ne retourne rien (void)",
                nameToken.Line,
                nameToken.Column);
        }

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

    private Statement ParseReturnStatement()
    {
        var keywordToken = Expect(TokenType.Return);

        if (!_insideFunction)
        {
            throw new TlCompilerException(
                "'return' ne peut être utilisé qu'à l'intérieur d'une fonction",
                keywordToken.Line,
                keywordToken.Column);
        }

        Expression? value = Current.Type == TokenType.Semicolon ? null : ParseExpression();
        Expect(TokenType.Semicolon);

        if (_currentFunctionReturnType is TlType expected)
        {
            if (value is null)
            {
                throw new TlCompilerException(
                    $"Cette fonction doit retourner {Describe(expected)}",
                    keywordToken.Line,
                    keywordToken.Column);
            }

            if (value.ResolveType() != expected)
            {
                throw new TlCompilerException(
                    $"Type de retour invalide : attendu {Describe(expected)}, trouvé {Describe(value.ResolveType())}",
                    keywordToken.Line,
                    keywordToken.Column);
            }
        }
        else if (value is not null)
        {
            throw new TlCompilerException(
                "Cette fonction ne retourne rien (void), 'return' ne doit pas avoir de valeur",
                keywordToken.Line,
                keywordToken.Column);
        }

        return new ReturnStatement(value);
    }

    private Statement ParseFunctionDeclaration()
    {
        Expect(TokenType.Fn);
        var nameToken = Expect(TokenType.Identifier);

        if (_functions.ContainsKey(nameToken.Lexeme))
        {
            throw new TlCompilerException(
                $"Fonction '{nameToken.Lexeme}' déjà déclarée",
                nameToken.Line,
                nameToken.Column);
        }

        Expect(TokenType.LParen);
        var parameters = ParseParameters();
        Expect(TokenType.RParen);

        TlType? returnType = null;

        if (Current.Type == TokenType.Arrow)
        {
            _position++;
            returnType = ParseTypeAnnotation();
        }

        // Enregistrée avant de parser le corps : permet la récursion directe.
        _functions[nameToken.Lexeme] = new FunctionSignature(parameters.Select(p => p.Type).ToList(), returnType);

        var savedScopes = _scopes;
        var savedInsideFunction = _insideFunction;
        var savedReturnType = _currentFunctionReturnType;

        _scopes = new List<Dictionary<string, TlType>> { new() };

        foreach (var parameter in parameters)
        {
            _scopes[0][parameter.Name] = parameter.Type;
        }

        _insideFunction = true;
        _currentFunctionReturnType = returnType;

        var body = ParseBlock();

        _scopes = savedScopes;
        _insideFunction = savedInsideFunction;
        _currentFunctionReturnType = savedReturnType;

        return new FunctionDeclaration(nameToken.Lexeme, parameters, returnType, body);
    }

    private List<Parameter> ParseParameters()
    {
        var parameters = new List<Parameter>();
        var seenNames = new HashSet<string>();

        if (Current.Type == TokenType.RParen)
        {
            return parameters;
        }

        parameters.Add(ParseParameter(seenNames));

        while (Current.Type == TokenType.Comma)
        {
            _position++;
            parameters.Add(ParseParameter(seenNames));
        }

        return parameters;
    }

    private Parameter ParseParameter(HashSet<string> seenNames)
    {
        var nameToken = Expect(TokenType.Identifier);

        if (!seenNames.Add(nameToken.Lexeme))
        {
            throw new TlCompilerException($"Paramètre '{nameToken.Lexeme}' déjà utilisé", nameToken.Line, nameToken.Column);
        }

        Expect(TokenType.Colon);
        var type = ParseTypeAnnotation();
        return new Parameter(nameToken.Lexeme, type);
    }

    private TlType ParseTypeAnnotation()
    {
        var token = Current;

        var type = token.Type switch
        {
            TokenType.TypeInt => TlType.Int,
            TokenType.TypeString => TlType.String,
            TokenType.TypeBool => TlType.Bool,
            _ => throw new TlCompilerException($"Type attendu, trouvé {token.Type} ('{token.Lexeme}')", token.Line, token.Column),
        };

        _position++;
        return type;
    }

    private Statement ParseCallStatement()
    {
        var call = ParseCall();
        Expect(TokenType.Semicolon);
        return new ExpressionStatement(call);
    }

    private CallExpression ParseCall()
    {
        var nameToken = Expect(TokenType.Identifier);
        Expect(TokenType.LParen);

        var arguments = new List<Expression>();

        if (Current.Type != TokenType.RParen)
        {
            arguments.Add(ParseExpression());

            while (Current.Type == TokenType.Comma)
            {
                _position++;
                arguments.Add(ParseExpression());
            }
        }

        Expect(TokenType.RParen);

        if (!_functions.TryGetValue(nameToken.Lexeme, out var signature))
        {
            throw new TlCompilerException($"Fonction '{nameToken.Lexeme}' non déclarée", nameToken.Line, nameToken.Column);
        }

        if (arguments.Count != signature.ParameterTypes.Count)
        {
            throw new TlCompilerException(
                $"'{nameToken.Lexeme}' attend {signature.ParameterTypes.Count} argument(s), {arguments.Count} fourni(s)",
                nameToken.Line,
                nameToken.Column);
        }

        for (var i = 0; i < arguments.Count; i++)
        {
            var expected = signature.ParameterTypes[i];
            var actual = arguments[i].ResolveType();

            if (actual != expected)
            {
                throw new TlCompilerException(
                    $"Argument {i + 1} de '{nameToken.Lexeme}' : attendu {Describe(expected)}, trouvé {Describe(actual)}",
                    nameToken.Line,
                    nameToken.Column);
            }
        }

        return new CallExpression(nameToken.Lexeme, arguments, signature.ReturnType ?? TlType.Void);
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
            case TokenType.Identifier when PeekNext().Type == TokenType.LParen:
                return ParseCall();
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
        TlType.Void => "rien (void)",
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

    private Token PeekNext() => _tokens[Math.Min(_position + 1, _tokens.Count - 1)];

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

    private sealed record FunctionSignature(IReadOnlyList<TlType> ParameterTypes, TlType? ReturnType);
}
