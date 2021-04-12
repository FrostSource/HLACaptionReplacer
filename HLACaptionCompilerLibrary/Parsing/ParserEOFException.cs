using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parsing
{
    class ParserEOFException : ParserException
    {
        public ParserEOFException()
        {
        }

        public ParserEOFException(string message, GenericToken encounteredToken = null)
            : base(BuildExceptionMessage(message, encounteredToken))
        {
        }

        public ParserEOFException(string message, Exception inner)
            : base(message, inner)
        {
        }

        private static string BuildExceptionMessage(string message, GenericToken encounteredToken)
        {
            if (encounteredToken == null)
                return $"{message}.";
            else
                return $"{message} at line {encounteredToken.LineNumber}, pos {encounteredToken.LinePosition}.";
        }
    }
}
