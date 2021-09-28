using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Expressions
{
    public class ConditionalExpression : TypedBinaryOperator
    {
        private readonly Dictionary<(Type, Type), Type> _typeRules;

        public ConditionalExpression(Token token, TypedExpression leftExpression, TypedExpression rightExpression) : base(token, leftExpression, rightExpression, null)
        {
            _typeRules = new Dictionary<(Type, Type), Type>
            {
                { (Type.Float, Type.Float), Type.Bool },
                { (Type.Int, Type.Int), Type.Bool },
                { (Type.Float, Type.Int), Type.Bool },
                { (Type.Int, Type.Float), Type.Bool },
                { (Type.Bool, Type.Bool), Type.Bool }

            };
        }

        public override dynamic Evaluate()
        {
            return Token.TokenType switch
            {
                TokenType.OrCondition => LeftExpression.Evaluate() || RightExpression.Evaluate(),
                TokenType.AndCondition => LeftExpression.Evaluate() && RightExpression.Evaluate(),
                TokenType.Not => !LeftExpression.Evaluate(),
                _ => throw new NotImplementedException()
            };
        }

        public override string Generate()
        {
            if (LeftExpression.GetExpressionType() == Type.String &&
                RightExpression.GetExpressionType() != Type.String)
            {
                return $"{LeftExpression.Generate()} {Token.Lexeme} {RightExpression.Generate()}";
            }

            return $"{LeftExpression.Generate()} {Token.Lexeme} {RightExpression.Generate()}";
        }

        public override Type GetExpressionType()
        {
            if (_typeRules.TryGetValue((LeftExpression.GetExpressionType(), RightExpression.GetExpressionType()), out var resultType))
            {
                return resultType;
            }
            throw new ApplicationException($"Cannot perform arithmetic operation on {LeftExpression.GetExpressionType()}, {RightExpression.GetExpressionType()}");

        }
    }
}
