using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parsing
{
    class TokenizerEOFException : TokenizerException
    {
        public TokenizerEOFException()
        {
        }

        public TokenizerEOFException(string message)
            : base("Unexpected end of file. " + message)
        {
        }

        public TokenizerEOFException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
