using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parsing
{
    public enum TokenType
    {
        Identifier,
        Symbol,
        String,
        Number,
        Integer,
        Float,
        Decimal,
        Boolean,
        WhiteSpace,
        Comment
    }
    public class GenericToken
    {
        public TokenType TokenType { get; private set; }
        public string Value { get; private set; }
        public int Index { get; private set; }
        public int LineNumber { get; private set; }
        public int LinePosition { get; private set; }

        public GenericToken(TokenType tokenType, string value, int index, int lineNumber, int linePosition)
        {
            TokenType = tokenType;
            Value = value;
            Index = index;
            LineNumber = lineNumber;
            LinePosition = linePosition;
        }

    }
}
