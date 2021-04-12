using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parsing
{
    class TokenizerSyntaxException : TokenizerException
    {
        public TokenizerSyntaxException()
        {
        }

        public TokenizerSyntaxException(string message, int line, int position)
            : base(SyntaxErrorMessage(message, line, position))
        {
        }

        public TokenizerSyntaxException(string message, int line, int position, Exception inner)
            : base(SyntaxErrorMessage(message, line, position), inner)
        {
        }

        private static string SyntaxErrorMessage(string message, int line, int position)
        {
            return $"Syntax Error: {message} at line {line}, pos {position}.";
        }
    }
}
