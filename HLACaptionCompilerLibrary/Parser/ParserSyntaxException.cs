using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    class ParserSyntaxException : ParserException
    {
        public ParserSyntaxException()
        {
        }

        public ParserSyntaxException(string message, int line, int position)
            : base(SyntaxErrorMessage(message, line, position))
        {
        }

        public ParserSyntaxException(string message, int line, int position, Exception inner)
            : base(SyntaxErrorMessage(message, line, position), inner)
        {
        }

        private static string SyntaxErrorMessage(string message, int line, int position)
        {
            return $"{message} at line {line}, pos {position}.";
        }
    }
}
