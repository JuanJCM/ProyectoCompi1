using Compiler.Core;
using Compiler.Core.Expressions;
using Compiler.Core.Interfaces;
using Compiler.Core.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using Type = Compiler.Core.Type;

namespace Compiler.Parser
{
    public class Parser : IParser
    {
        private readonly IScanner scanner;
        private Token lookAhead;

        public Parser(IScanner scanner)
        {
            this.scanner = scanner;
            this.Move();
        }

        public Statement Parse()
        {
            return Program();
        }

        private Statement Program()
        {

            EnvironmentManager.PushContext();
            EnvironmentManager.AddMethod("print", new Id(new Token
            {
                Lexeme = "print",
            }, Type.Void),
            new ArgumentExpression(new Token
            {
                Lexeme = ""
            },
            new Id(new Token
            {
                Lexeme = "arg1"
            }, Type.String)));
            var block = Block();
            block.ValidateSemantic();
            var code = block.Generate(0);
            Console.WriteLine(code);
            return block;
        }
        private Statement Class()
        {
            if(lookAhead.TokenType != TokenType.Class)
            {
                throw new ApplicationException($"Must first initiate with a class");

            }
            Match(TokenType.Class);
            var expression = Stmt();
            Match(TokenType.Identifier);
            Match(TokenType.OpenBrace);

            return new ClassStatement(expression);
        }
        private Statement Block()
        {
            Match(TokenType.OpenBrace);
            EnvironmentManager.PushContext();
            Decls();
            var statements = Stmts();
            Match(TokenType.CloseBrace);
            EnvironmentManager.PopContext();
            return statements;
        }

        private Statement Stmts()
        {
            if (this.lookAhead.TokenType == TokenType.CloseBrace)
            {
                return null;
            }
            return new SequenceStatement(Stmt(), Stmts());
        }

        private Statement Stmt()
        {
            Expression expression;
            Statement statement1, statement2;
            switch (this.lookAhead.TokenType)
            {
                case TokenType.Identifier:
                    {
                        var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                        Match(TokenType.Identifier);
                        if (this.lookAhead.TokenType == TokenType.Assignation)
                        {
                            return AssignStmt(symbol.Id);
                        }
                        /*else
                        {
                            if (lookAhead.TokenType == TokenType.Increment)
                            {   
                                var logic = Logical();
                                return IncDecExpression(logic as TypedExpression, TokenType.Increment);
                            }
                            else
                            {
                                if (lookAhead.TokenType == TokenType.Decrement)
                                {
                                    var logic2 = Logical();
                                    Match(TokenType.Decrement);
                                    return IncDecExpression( logic2 as TypedExpression, TokenType.Decrement);
                                }
                            }
                        }*/
                            return CallStmt(symbol);
                    }
                case TokenType.IfKeyword:
                    {
                        Match(TokenType.IfKeyword);
                        Match(TokenType.LeftParens);

                        expression = Logical();
                        Match(TokenType.RightParens);
                        statement1 = Stmt();
                        if (this.lookAhead.TokenType != TokenType.ElseKeyword)
                        {
                            return new IfStatement(expression as TypedExpression, statement1);
                        }
                        Match(TokenType.ElseKeyword);
                        statement2 = Stmt();
                        return new ElseStatement(expression as TypedExpression, statement1, statement2);
                    }
                case TokenType.WhileKeyword:
                    {
                        Match(TokenType.WhileKeyword);
                        Match(TokenType.LeftParens);

                        expression = Logical();
                        Match(TokenType.RightParens);
                        statement1 = Block(); 

                        return new WhileStatement(expression as TypedExpression, statement1);
                    }
                case TokenType.ForeachKeyword:
                    {
                        Match(TokenType.ForeachKeyword);
                        Match(TokenType.LeftParens);
                        expression = Logical();
                        Match(TokenType.InKeyword);
                        var lista = Logical();                        
                        Match(TokenType.RightParens);
                        statement1 = Block();
                        return new ForeachStatement(expression as TypedExpression, statement1 , lista as TypedExpression);
                    }
                case TokenType.Void:
                    {
                        var symbol2 = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme); 
                        Match(TokenType.Void);
                        Match(TokenType.Identifier);
                        Match(TokenType.OpenBrace);
                        var @params = OptParams();
                        Match(TokenType.CloseBrace);
                        return new MethodStatement(symbol2.Id, @params, symbol2.Attributes);
                    }
                case TokenType.ListKeyword:
                    {
                        Match(TokenType.ListKeyword);
                        Match(TokenType.LessThan);
                        switch (lookAhead.TokenType)
                        {
                            case TokenType.IntKeyword:
                                Match(TokenType.IntKeyword);
                                break;
                            case TokenType.FloatKeyword:
                                Match(TokenType.FloatKeyword);
                                break;
                            case TokenType.BoolKeyword:
                                Match(TokenType.BoolKeyword);
                                break;
                            default:
                                throw new ApplicationException($"Type not allowed {this.lookAhead.TokenType}. Line: {this.lookAhead.Line}, Column: {this.lookAhead.Column}");

                        }
                        Match(TokenType.GreaterThan);
                        expression = Logical();
                        Match(TokenType.SemiColon);
                        var block = Block();
                        return new ListStatement(expression as TypedExpression, block) ;
                    }

                default:
                    return Block();
            }
        }
        private Statement IncDecExpression( TypedExpression Logic, TokenType type)
        {
            Match(TokenType.Increment);
            Match(TokenType.SemiColon);
            return new IncDecStatement( Logic, type);
        }
        private Expression Logical()
        {
            var expression = Eq();
            while (this.lookAhead.TokenType == TokenType.OrCondition || this.lookAhead.TokenType == TokenType.AndCondition)
            {
                var token = lookAhead;
                Move();
                expression = new ConditionalExpression(token, expression as TypedExpression, Eq() as TypedExpression);
                
            }

            return expression;
        }

        private Expression Eq()
        {
            var expression = Rel();
            while (this.lookAhead.TokenType == TokenType.Equal || this.lookAhead.TokenType == TokenType.NotEqual)
            {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Rel() as TypedExpression);
            }

            return expression;
        }

        private Expression Rel()
        {
            var expression = Expr();
            if (this.lookAhead.TokenType == TokenType.LessThan
                || this.lookAhead.TokenType == TokenType.GreaterThan
                || this.lookAhead.TokenType == TokenType.LessOrEqualThan
                || this.lookAhead.TokenType == TokenType.GreaterOrEqualThan)
            {
                var token = lookAhead;
                Move();
                expression = new RelationalExpression(token, expression as TypedExpression, Expr() as TypedExpression);
            }
            return expression;
        }

        private Expression Expr()
        {
            var expression = Term();
            while (this.lookAhead.TokenType == TokenType.Plus || this.lookAhead.TokenType == TokenType.Minus)
            {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Term() as TypedExpression);
            }
            return expression;
        }

        private Expression Term()
        {
            var expression = Factor();
            while (this.lookAhead.TokenType == TokenType.Asterisk || this.lookAhead.TokenType == TokenType.Division || this.lookAhead.TokenType == TokenType.Percentage)
            {
                var token = lookAhead;
                Move();
                expression = new ArithmeticOperator(token, expression as TypedExpression, Factor() as TypedExpression);
            }
            return expression;
        }

        private Expression Factor()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.LeftParens:
                    {
                        Match(TokenType.LeftParens);
                        var expression = Logical();
                        Match(TokenType.RightParens);
                        return expression;
                    }
                case TokenType.IntConstant:
                    var constant = new Constant(lookAhead, Type.Int);
                    Match(TokenType.IntConstant);
                    return constant;
                case TokenType.FloatConstant:
                    constant = new Constant(lookAhead, Type.Float);
                    Match(TokenType.FloatConstant);
                    return constant;
                case TokenType.StringConstant:
                    constant = new Constant(lookAhead, Type.String);
                    Match(TokenType.StringConstant);
                    return constant;
                case TokenType.BoolConstant:
                    constant = new Constant(lookAhead, Type.Bool);
                    Match(TokenType.BoolConstant);
                    return constant;
                case TokenType.Not:
                    Match(TokenType.Not);
                    return Logical();
                case TokenType.DateTimeKeyword:
                    var token2 = this.lookAhead;
                    List<Token> list = new List<Token>();
                    Match(TokenType.DateTimeKeyword);
                    list.Add(lookAhead);
                    Match(TokenType.LeftParens);
                    list.Add(lookAhead);
                    Match(TokenType.IntConstant);
                    Match(TokenType.Division);
                    list.Add(lookAhead);
                    Match(TokenType.IntConstant);
                    Match(TokenType.Division); 
                    list.Add(lookAhead);
                    Match(TokenType.IntConstant);
                    list.Add(lookAhead);
                    Match(TokenType.RightParens);
                    return new ConstantExpression(token2, Type.Date, string.Concat(list.Select(x => x.Lexeme)));
                case TokenType.IntListConstant:
                    token2 = this.lookAhead;
                    list = new List<Token>();
                    Match(TokenType.IntListConstant);
                    list.Add(lookAhead);
                    Match(TokenType.LeftParens);
                    list.Add(lookAhead);
                    Match(TokenType.IntConstant);
                    while (lookAhead.TokenType != TokenType.RightParens)
                    {
                        list.Add(lookAhead);
                        Match(TokenType.Comma);
                        list.Add(lookAhead);
                        Match(TokenType.IntConstant);
                    }
                    list.Add(lookAhead);
                    Match(TokenType.RightParens);
                    return new ConstantExpression(null, Type.Int, string.Concat(list.Select(x => x.Lexeme)));
                case TokenType.FloatListConstant:
                    list = new List<Token>();
                    list.Add(lookAhead);
                    Match(TokenType.FloatListConstant);
                    list.Add(lookAhead);
                    Match(TokenType.LeftParens);
                    list.Add(lookAhead);
                    Match(TokenType.FloatConstant);
                    while (lookAhead.TokenType != TokenType.RightParens)
                    {
                        list.Add(lookAhead);
                        Match(TokenType.Comma);
                        list.Add(lookAhead);
                        Match(TokenType.FloatConstant);
                    }
                    list.Add(lookAhead);
                    Match(TokenType.RightParens);
                    return new ConstantExpression(null, Type.Float, string.Concat(list.Select(x => x.Lexeme)));
                case TokenType.BoolListConstant:
                    list = new List<Token>();
                    list.Add(lookAhead);
                    Match(TokenType.BoolListConstant);
                    list.Add(lookAhead);
                    Match(TokenType.LeftParens);
                    list.Add(lookAhead);
                    Match(TokenType.BoolConstant);
                    while (lookAhead.TokenType != TokenType.RightParens)
                    {
                        list.Add(lookAhead);
                        Match(TokenType.Comma);
                        list.Add(lookAhead);
                        Match(TokenType.BoolConstant);
                    }
                    list.Add(lookAhead);
                    Match(TokenType.RightParens);
                    return new ConstantExpression(null, Type.Bool, string.Concat(list.Select(x => x.Lexeme)));
                default:
                    var symbol = EnvironmentManager.GetSymbol(this.lookAhead.Lexeme);
                    Match(TokenType.Identifier);
                    return symbol.Id;
            }
        }

        private Statement CallStmt(Symbol symbol)
        {
            Match(TokenType.LeftParens);
            var @params = OptParams();
            Match(TokenType.RightParens);
            Match(TokenType.SemiColon);
            return new CallStatement(symbol.Id, @params, symbol.Attributes);
        }

        private Expression OptParams()
        {
            if (this.lookAhead.TokenType != TokenType.RightParens)
            {
                return Params();
            }
            return null;
        }

        private Expression Params()
        {
            var expression = Logical();
            if (this.lookAhead.TokenType != TokenType.Comma)
            {
                return expression;
            }
            Match(TokenType.Comma);
            expression = new ArgumentExpression(lookAhead, expression as TypedExpression, Params() as TypedExpression);
            return expression;
        }

        private Statement AssignStmt(Id id)
        {

            Match(TokenType.Assignation);
            var expression = Logical();
            Match(TokenType.SemiColon); 
            return new AssignationStatement(id, expression as TypedExpression);
        }

        private void Decls()
        {
            if (this.lookAhead.TokenType == TokenType.IntKeyword || 
                this.lookAhead.TokenType == TokenType.ListKeyword ||
                this.lookAhead.TokenType == TokenType.FloatKeyword ||
                this.lookAhead.TokenType == TokenType.StringKeyword ||
                this.lookAhead.TokenType == TokenType.BoolKeyword ||
                this.lookAhead.TokenType == TokenType.DateTimeKeyword
                )
            {
                Decl();
                Decls();
            }
        }

        private void Decl()
        {
            switch (this.lookAhead.TokenType)
            {
                case TokenType.FloatKeyword:
                    Match(TokenType.FloatKeyword);
                    var token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    var id = new Id(token, Type.Float);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;

                case TokenType.StringKeyword:
                    Match(TokenType.StringKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.String);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;

                case TokenType.DateTimeKeyword:
                    Match(TokenType.DateTimeKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Date);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
                case TokenType.BoolKeyword:
                    Match(TokenType.BoolKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Bool);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;

                default:
                    Match(TokenType.IntKeyword);
                    token = lookAhead;
                    Match(TokenType.Identifier);
                    Match(TokenType.SemiColon);
                    id = new Id(token, Type.Int);
                    EnvironmentManager.AddVariable(token.Lexeme, id);
                    break;
            }
        }

        private void Move()
        {
            this.lookAhead = this.scanner.GetNextToken();
        }

        private void Match(TokenType tokenType)
        {
            if (this.lookAhead.TokenType != tokenType)
            {
                throw new ApplicationException($"Syntax error! expected token {tokenType} but found {this.lookAhead.TokenType}. Line: {this.lookAhead.Line}, Column: {this.lookAhead.Column}");
            }
            this.Move();
        }
    }
}
