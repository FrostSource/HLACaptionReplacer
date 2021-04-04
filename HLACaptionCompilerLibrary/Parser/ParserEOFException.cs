using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    class ParserEOFException : Exception
    {
        public ParserEOFException()
        {
        }

        public ParserEOFException(string message)
            : base(message)
        {
        }

        public ParserEOFException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
