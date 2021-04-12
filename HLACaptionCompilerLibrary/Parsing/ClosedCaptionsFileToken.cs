using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    public class ClosedCaptionsFileToken : GenericToken
    {
        public int PreDirectiveIndex { get; private set; }
        public int PreDirectiveLineNumber { get; private set; }
        public int PreDirectiveLinePosition { get; private set; }

        public ClosedCaptionsFileToken(TokenType tokenType, string value, int index, int lineNumber, int linePosition,
                                int preDirectiveIndex = -1, int preDirectiveLineNumber = -1, int preDirectiveLinePosition = -1) : base(tokenType, value, index, lineNumber, linePosition)
        {
            PreDirectiveIndex = preDirectiveIndex;
            PreDirectiveLineNumber = preDirectiveLineNumber;
            PreDirectiveLinePosition = preDirectiveLinePosition;
        }
    }
}
