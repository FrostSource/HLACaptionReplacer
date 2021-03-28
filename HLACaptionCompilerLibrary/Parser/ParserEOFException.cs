using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionReplacer.Parser
{
    class ParserEOFException : ParserException
    {
        public ParserEOFException()
        {
        }

        public ParserEOFException(string message)
            : base("Unexpected end of file. " + message)
        {
        }

        public ParserEOFException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
