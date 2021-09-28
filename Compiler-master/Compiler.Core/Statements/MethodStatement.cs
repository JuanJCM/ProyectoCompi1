using Compiler.Core.Expressions;
using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Statements
{
    public class MethodStatement : Statement
    {
        public MethodStatement(Id id, Expression arguments, Expression attributes)
        {
            Arguments = arguments;
            Attributes = attributes;
            Id = id;
        }

        public Expression Arguments { get; }
        public Expression Attributes { get; }
        public Id Id { get; }

        public override string Generate(int tabs)
        {
            var code = GetCodeInit(tabs);
            var innerCode = InnerCodeGenerateCode(Arguments);
            code += $"{Id.Generate()}({innerCode}){Environment.NewLine}";
            return code;
        }

        private object InnerCodeGenerateCode(Expression arguments)
        {
            var code = string.Empty;
            if (arguments is BinaryOperator binary)
            {
                code += InnerCodeGenerateCode(binary.LeftExpression);
                code += InnerCodeGenerateCode(binary.RightExpression);
            }
            else
            {
                code += arguments.Generate();
            }
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
