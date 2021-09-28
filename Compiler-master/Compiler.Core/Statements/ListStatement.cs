using Compiler.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Statements
{
    public class ListStatement: Statement
    {
        public ListStatement(TypedExpression expression, Statement block)
        {
            Expression = expression;
            Block = block;
        }

        public TypedExpression Expression { get; }
        public Statement Block { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            code += $"List<> = {Expression.Generate()};{Environment.NewLine}";
            return code;
        }

        public override void Interpret()
        {
            throw new NotImplementedException();
        }

        public override void ValidateSemantic()
        {
            throw new NotImplementedException();
        }
    }
}
