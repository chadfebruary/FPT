using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPT.Parsing
{
    public enum TokenType
    {
        Add,
        Subtract,
        Multiply,
        Divide,
        SequenceTerminator,
        Match,
        //In,
        //NotIn,
        Application,
        ExceptionType,
        FingerPrint,
        Message,
        StackFrame,
        Equals,
        NotEquals,
        Like,
        NotLike,
        StringValue,
        OpenParenthesis,
        CloseParenthesis,
        Comma,
        And,
        Or,
        Between,
        Limit,
        Number
    }
}
