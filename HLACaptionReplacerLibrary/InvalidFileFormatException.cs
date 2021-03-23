using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionReplacer
{
    public class InvalidFileFormatException : Exception
    {
        public InvalidFileFormatException()
        {
        }

        public InvalidFileFormatException(string message) : base(message)
        {
        }

        public InvalidFileFormatException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected InvalidFileFormatException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
