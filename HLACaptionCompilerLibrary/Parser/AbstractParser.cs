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
        public char CurrentChar { get; private set; }
        public int Index { get; private set; } = 0;
        public int LineNumber { get; private set; } = 0;
        public int LinePosition { get; private set; } = 1;
        public bool AutoSkipWhitespace { get; set; } = true;

        public AbstractParser(string source)
        {

        }
        /// <summary>
        /// Advances the next character in the source text.
        /// </summary>
        /// <exception cref="ParserEOFException">Thrown when the end of the source is found instead of a character.</exception>
        public void Advance()
        {
            if (Index >= Source.Length) throw new ParserEOFException();
            if (Source[Index] == '\n')
            {
                LineNumber++;
                LinePosition = 1;
            }
            Index++;
        }
        /// <summary>
        /// Advances the given number of characters in the source text.
        /// </summary>
        /// <param name="count"></param>
        /// <exception cref="ParserEOFException">Thrown when the end of the source is found instead of a character.</exception>
        public void Advance(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Advance();
            }
        }
        /// <summary>
        /// Returns the next <see cref="char"/> in the source text and advances.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ParserEOFException">Thrown when the end of the source is found instead of a character.</exception>
        public char Next()
        {
            if (AutoSkipWhitespace) SkipWhitespace();
            if (Index >= Source.Length) return '\0';
            Advance();
            return CurrentChar;
        }
        /// <summary>
        /// Returns the next <see cref="char"/> in the source text without advancing.
        /// </summary>
        /// <returns></returns>
        public char Peek()
        {
            var index = Index;
            if (AutoSkipWhitespace) index = NextIndexAfterWhitespace();
            if (index >= Source.Length) return '\0';
            return Source[index];
        }
        /// <summary>
        /// Checks if <paramref name="str"/> is next in the current position of the source text.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>True if <paramref name="str"/> is next in the current position of the source text.</returns>
        public bool IsNext(string str)
        {
            if (str.Length > Source.Length - Index) return false;

            if (Source.Substring(Index, str.Length) == str)
            {
                return true;
            }
            return false;
        }
        /// <summary>
        /// Eats the given <see cref="string"/> at the front of the source text.
        /// </summary>
        /// <param name="str"></param>
        /// <exception cref="ParserEOFException">Thrown when the parser unexpectedly reaches the end of the source.</exception>
        /// <exception cref="ParserSyntaxException">Thrown when <paramref name="str"/> is not next in the source.</exception>
        public void Eat(string str)
        {
            if (str.Length > Source.Length - Index)
            {
                throw new ParserEOFException();
            }
            if (!IsNext(str))
            {
                SyntaxError($"Expected '{str}'");
            }
            Advance(str.Length);
        }
        /// <summary>
        /// Returns the next <see cref="string"/> enclosed between a given <see cref="char"/>.
        /// </summary>
        /// <param name="boundaryChar">The <see cref="char"/> that defines the boundary.</param>
        /// <returns></returns>
        /// <exception cref="ParserEOFException">Thrown when the end of the source is found instead of a character.</exception>
        /// <exception cref="ParserSyntaxException">Thrown when <paramref name="boundaryChar"/> is not at the beginning or end.</exception>
        public string NextEnclosed(char boundaryChar = '"')
        {
            if (Next() != boundaryChar)
            {
                SyntaxError($"Expected {boundaryChar}");
            }
            // Previous values saved for more accurate error reporting
            var linePrev = LineNumber;
            var posPrev = LinePosition;
            var str = new StringBuilder();
            char ch;
            while ((ch = Next()) != boundaryChar)
            {
                if (ch == '\0') SyntaxError($"Missing closing '{boundaryChar}'", linePrev, posPrev);
                //TODO: Allow escaped characters? Does Valve allow them? E.g. \<
                str.Append(ch);
            }
            return ch.ToString();
        }
        /// <summary>
        /// Gets the next integer in the source text
        /// </summary>
        /// <returns></returns>
        public string NextInteger()
        {
            // Previous values saved for more accurate error reporting
            var linePrev = LineNumber;
            var posPrev = LinePosition;
            var str = new StringBuilder();
            char ch;
            while (IsDigit(ch = Next()))
            {
                str.Append(ch);
            }
            if (str.Length == 0)
            {
                SyntaxError("Expecting integer");
            }
            return ch.ToString();
        }
        /// <summary>
        /// Advances the source string to the first non-whitespace character.
        /// </summary>
        public void SkipWhitespace()
        {
            while (Peek() != '\0' && Char.IsWhiteSpace(Peek()))
            {
                Advance();
            }
        }
        public int NextIndexAfterWhitespace()
        {
            var index = Index;
            while (index < Source.Length && Char.IsWhiteSpace(Source[index]))
            {
                index++;
            }
            return index;
        }

        public string LinePositionFormatted()
        {
            return $"line {LineNumber}, pos {LinePosition}";
        }
        public static string LinePositionFormatted(int lineNumber, int linePosition)
        {
            return $"line {lineNumber}, pos {linePosition}";
        }
        public void SyntaxError(string message)
        {
            throw new ParserSyntaxException(message, LineNumber, LinePosition);
        }
        public void SyntaxError(string message, int lineNumber, int linePosition)
        {
            throw new ParserSyntaxException(message, lineNumber, linePosition);
        }

        public static bool IsDigit(char ch)
        {
            return (ch == '0' || ch == '1' || ch == '2' || ch == '3' || ch == '4' || ch == '5' || ch == '6' || ch == '7' || ch == '8' || ch == '9');
        }

        public abstract IDictionary<string, dynamic> Parse();
    }
}
