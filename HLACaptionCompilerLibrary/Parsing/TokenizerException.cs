using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parsing
{
    class TokenizerException : Exception
    {
        public TokenizerException()
        {
        }

        public TokenizerException(string message)
            : base(message)
        {
        }

        public TokenizerException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
