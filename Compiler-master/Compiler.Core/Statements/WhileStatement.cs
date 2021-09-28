using Compiler.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Statements
{
    public class WhileStatement : Statement
    {
        public WhileStatement(TypedExpression expression, Statement block)
        {
            Expression = expression;
            Block = block;
        }

        public TypedExpression Expression { get; }
        public Statement Block { get; }

        public override string Generate(int tab)
        {
            var code = GetCodeInit(tab);
            code += $"while({Expression.Generate()}){Environment.NewLine}{{{Environment.NewLine} ";
            code += $"{Block.Generate(tab +1)}{Environment.NewLine}}}{Environment.NewLine} ";
            return code;
        }

        public override void Interpret()
        {
            if (Expression.Evaluate())
            {
                Block.Interpret();
            }
        }

        public override void ValidateSemantic()
        {
            if (Expression.GetExpressionType() != Type.Bool)
            {
                throw new ApplicationException("A boolean is required in while");
            }
        }
    }
}
