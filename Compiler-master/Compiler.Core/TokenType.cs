using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler.Core
{
    public enum TokenType
    {
        Asterisk,
        Plus,
        Minus,
        LeftParens,
        RightParens,
        OpenBracket,
        CloseBracket,
        SemiColon,
        Equal,
        Division,
        LessThan,
        LessOrEqualThan,
        NotEqual,
        GreaterThan,
        GreaterOrEqualThan,
        IntKeyword,
        IfKeyword,
        ElseKeyword,
        Identifier,
        IntConstant,
        FloatConstant,
        Assignation,
        EOF,
        OpenBrace,
        CloseBrace,
        Comma,
        BasicType,
        FloatKeyword,
        StringKeyword,
        ForeachKeyword,
        WhileKeyword,
        BoolKeyword,
        BoolConstant,
        TrueKeyword,
        FalseKeyword,
        DateTimeKeyword,
        DateTimeConstant,
        IntListKeyword,
        FloatListKeyword,
        IntListConstant,
        FloatListConstant,
        StringConstant,
        AndCondition,
        OrCondition,
        Increment,
        Decrement,
        InKeyword,
        Not,
        Percentage,
        Void,
        Class,
        Day,
        Year,
        Month,
        New,
        BoolList,
        BoolListConstant,
        ListKeyword,
      



    }
}
