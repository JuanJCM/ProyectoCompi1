using Compiler.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Statements
{
    public class IncDecStatement: Statement
    {
        public IncDecStatement( TypedExpression expression, TokenType type)
        {

            Expression = expression;
            Type = type;
        }

        public Id Id { get; }
        public TypedExpression Expression { get; }
        public TokenType Type { get; }

        public override string Generate(int tab)
        {
            var code = "";
            if(Type== TokenType.Increment)
            {
                code = $"{Id.Generate()}++;";
            }
            else
            {
                if (Type == TokenType.Decrement)
                    code = $"{Id.Generate()}--";
            }
            return code;
        }

        public override void Interpret()
        {
            //EnvironmentManager.UpdateVariable(Id.Token.Lexeme, Expression.Evaluate());
        }

        public override void ValidateSemantic()
        {
            if (Id.GetExpressionType() != Expression.GetExpressionType())
            {
                throw new ApplicationException($"Type {Id.GetExpressionType()} is not assignable to {Expression.GetExpressionType()}");
            }
        }
    }
}
