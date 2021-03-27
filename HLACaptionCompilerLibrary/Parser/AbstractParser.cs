using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionReplacer.Parser
{
    abstract class AbstractParser
    {
        public string Source { get; private set; }
        public int Index { get; private set; } = 0;
        public int LineNumber { get; private set; }
        public bool AutoSkipWhitespace { get; set; } = true;

        public AbstractParser(string source)
        {

        }
        /// <summary>
        /// Returns the next <see cref="char"/> in the source string and advances.
        /// </summary>
        /// <returns></returns>
        public char Next()
        {
            if (AutoSkipWhitespace) SkipWhitespace();
            if (Index >= Source.Length) return '\0';
            return Source[Index++];
        }
        /// <summary>
        /// Returns the next <see cref="char"/> in the source string without advancing.
        /// </summary>
        /// <returns></returns>
        public char Peek()
        {
            if (Index >= Source.Length) return '\0';
            return Source[Index];
        }
        /// <summary>
        /// Checks if <paramref name="str"/> is next in the current position of the source string, and advances past it if it is.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>True if <paramref name="str"/> is next in the current position of the source string.</returns>
        public bool IsNext(string str)
        {
            if (str.Length > Source.Length - Index) return false;

            if (Source.Substring(Index, str.Length) == str)
            {
                Index += str.Length;
                return true;
            }
            return false;
        }
        /// <summary>
        /// Advances the source string to the first non-whitespace character.
        /// </summary>
        public void SkipWhitespace()
        {
            while (Peek() != '\0' && Char.IsWhiteSpace(Peek()))
            {
                Next();
            }
        }

        public abstract IDictionary<string, dynamic> Parse();
    }
}
