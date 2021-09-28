using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core.Statements
{
    public class ClassStatement : Statement
    {
        public ClassStatement(Statement statement)
        {
            Statement = statement;
        }

        public Statement Statement { get; }

        public override string Generate(int tab)
        {
            var code = $"class {Statement.Generate(tab)}" + "{" + $"{Environment.NewLine}";
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
