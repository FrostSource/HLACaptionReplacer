﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    class ParserException : Exception
    {
        public ParserException()
        {
        }

        public ParserException(string message)
            : base(message)
        {
        }

        public ParserException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}