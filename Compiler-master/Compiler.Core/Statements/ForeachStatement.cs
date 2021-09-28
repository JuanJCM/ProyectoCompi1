using Compiler.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Statements
{
    public class ForeachStatement : Statement
    {


        public ForeachStatement(TypedExpression expression, Statement block, TypedExpression block2)
        {
            Expression = expression;
            Block = block;
            Block2 = block2;
        }

        public TypedExpression Expression { get; }
        public Statement Block { get; }
        public TypedExpression Block2 { get; }

        public override string Generate(int tab)
        {
            var code = GetCodeInit(tab);
            code += $"foreach({Expression.Generate()} in {Block2.Generate()}){Environment.NewLine}{{{Environment.NewLine} ";
            code += $"{Block.Generate(tab + 1)}{Environment.NewLine}}}{Environment.NewLine} ";
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
