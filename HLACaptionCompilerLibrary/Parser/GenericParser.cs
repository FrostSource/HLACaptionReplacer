using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    public class GenericParser
    {
        /// <summary>
        /// Gets or sets if white space should be skipped by the parser when finding elements such as numbers or words.
        /// </summary>
        public bool AutoSkipWhiteSpace { get; set; } = true;

        public string Source { get; private set; }
        public char CurrentChar { get => Source[Index]; }
        public int Index { get; private set; } = 0;
        public int LineNumber { get; private set; } = 1;
        public int LinePosition { get; private set; } = 1;
        public bool EOF { get => Index >= Source.Length; }
        public const string Letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public const string Digits = "1234567890";
        /// <summary>
        /// Gets or sets the chars that define the default boundary between sequences such as words and numbers.
        /// A sequence will stop when it encounters one of the following: White space, boundary char, EOF.
        /// </summary>
        public string BoundaryChars { get; set; } = ",.{}[]/-=+()!'\"";

        public GenericParser(string source)
        {
            Source = source;
        }
        /// <summary>
        /// Advances the next character in the source text.
        /// </summary>
        /// <exception cref="ParserEOFException">Thrown when the end of the source is found instead of a character.</exception>
        public void Advance()
        {
            if (EOF) throw new ParserEOFException();
            if (Source[Index] == '\n')
            {
                LineNumber++;
                LinePosition = 1;
            }
            else
            {
                LinePosition++;
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
        /// Get the next <see cref="char"/> in the source text and advances, skipping any white space at the beginning.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="ParserEOFException">Thrown when the end of the source is found instead of a character.</exception>
        public char Next()
        {
            if (AutoSkipWhiteSpace) SkipWhiteSpace();
            if (EOF) return '\0';
            char ch = CurrentChar;
            Advance();
            return ch;
        }
        /// <summary>
        /// Returns the next <see cref="char"/> in the source text without advancing.
        /// </summary>
        /// <returns></returns>
        public char Peek()
        {
            var index = Index;
            if (AutoSkipWhiteSpace) index = NextIndexAfterWhiteSpace();
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
            
            var index = Index;
            if (AutoSkipWhiteSpace) index = NextIndexAfterWhiteSpace();
            if (Source.Substring(index, str.Length) == str)
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
            if (AutoSkipWhiteSpace) SkipWhiteSpace();
            Advance(str.Length);
        }
        /// <summary>
        /// Returns the next <see cref="string"/> enclosed between a given <see cref="char"/>.
        /// <para>Does not return the matching <paramref name="boundaryChar"/>.</para>
        /// </summary>
        /// <param name="boundaryChar">The <see cref="char"/> that defines the boundary.</param>
        /// <returns></returns>
        /// <exception cref="ParserEOFException">Thrown when the end of the source is found instead of a character.</exception>
        /// <exception cref="ParserSyntaxException">Thrown when <paramref name="boundaryChar"/> is not at the beginning or end.</exception>
        public virtual string NextEnclosed(char boundaryChar = '"')
        {
            if (AutoSkipWhiteSpace) SkipWhiteSpace();
            if (Next() != boundaryChar)
            {
                SyntaxError($"Expected '{boundaryChar}'");
            }
            // Previous values saved for more accurate error reporting
            var linePrev = LineNumber;
            var posPrev = LinePosition;
            var str = new StringBuilder();
            while (CurrentChar != boundaryChar)
            {
                //if (ch == '\0') SyntaxError($"Missing closing '{boundaryChar}'", linePrev, posPrev);
                if (EOF) SyntaxError($"Missing closing '{boundaryChar}'", linePrev, posPrev);
                //TODO: Allow escaped characters? Does Valve allow them? E.g. \<
                str.Append(CurrentChar);
                Advance();
            }
            Advance();
            return str.ToString();
        }
        /// <summary>
        /// Gets the next non-whitespace string of characters in the source text.
        /// </summary>
        /// <returns></returns>
        public virtual string NextWord()
        {
            if (AutoSkipWhiteSpace) SkipWhiteSpace();
            var str = new StringBuilder();
            while (!EOF && !BoundaryChars.Contains(CurrentChar) && !IsWhiteSpace(CurrentChar))
            {
                str.Append(CurrentChar);
                Advance();
            }
            if (str.Length == 0)
            {
                SyntaxError("Expecting word");
            }
            return str.ToString();
        }
        /// <summary>
        /// Gets the next integer in the source text
        /// </summary>
        /// <returns></returns>
        public virtual string NextInteger()
        {
            if (AutoSkipWhiteSpace) SkipWhiteSpace();
            var str = new StringBuilder();
            while (!EOF && !BoundaryChars.Contains(CurrentChar) && !IsWhiteSpace(CurrentChar))
            {
                if (!IsDigit(CurrentChar)) SyntaxError($"Unexpected character '{CurrentChar}' in number");
                str.Append(CurrentChar);
                Advance();
            }
            if (str.Length == 0)
            {
                SyntaxError("Expecting integer");
            }
            return str.ToString();
        }
        /// <summary>
        /// Gets the next decimal number in the source text.
        /// </summary>
        /// <remarks>May return a similar result to <see cref="NextInteger"/> if no decimal point is found but a valid number is.</remarks>
        /// <returns></returns>
        public virtual string NextDecimal(char decimalChar = '.')
        {
            if (AutoSkipWhiteSpace) SkipWhiteSpace();
            var boundaryChars = BoundaryChars;
            int pos;
            if ((pos = BoundaryChars.IndexOf(decimalChar)) != -1)
                boundaryChars = BoundaryChars.Remove(pos, 1);
            var foundDecimalChar = false;
            var str = new StringBuilder();
            while (!EOF && !boundaryChars.Contains(CurrentChar) && !IsWhiteSpace(CurrentChar))
            {
                if (!IsDigit(CurrentChar) && CurrentChar != decimalChar) SyntaxError($"Unexpected character '{CurrentChar}' in number");
                if (CurrentChar == decimalChar)
                {
                    if (foundDecimalChar) SyntaxError($"Encountered more than one '{decimalChar}' in decimal number.");
                    foundDecimalChar = true;
                }
                str.Append(CurrentChar);
                Advance();
            }
            if (str.Length == 0)
            {
                SyntaxError("Expecting decimal number");
            }
            return str.ToString();
        }
        /// <summary>
        /// Gets a sequence of characters with a give ruleset of characters.
        /// </summary>
        /// <param name="validStartChars"><see cref="string"/> of characters that the sequence may have as its first character.</param>
        /// <param name="validChars"><see cref="string"/> of characters that the sequence may contain after the first character.</param>
        /// <returns></returns>
        /// <example>
        /// <code>
        /// // Getting a variable name that can't start with numbers but can contain them.
        /// var name = parser.NextSequence("_abc","_abc123");
        /// </code>
        /// </example>
        public virtual string NextSequence(string validStartChars, string validChars)
        {
            if (AutoSkipWhiteSpace) SkipWhiteSpace();
            if (!validStartChars.Contains(CurrentChar)) SyntaxError($"Expecting one of '{validStartChars}', found '{CurrentChar}'");
            Advance();

            var str = new StringBuilder();
            while (!EOF && !IsWhiteSpace(CurrentChar))
            {
                if (!validChars.Contains(CurrentChar)) SyntaxError($"Expecting one of '{validChars}', found '{CurrentChar}'");
                str.Append(CurrentChar);
                Advance();
            }
            if (str.Length == 0)
            {
                SyntaxError("Expecting sequence");
            }
            return str.ToString();
        }
        /// <summary>
        /// Advances the source string to the first non-whitespace character.
        /// </summary>
        public void SkipWhiteSpace()
        {
            while (!EOF && IsWhiteSpace(CurrentChar))
            {
                Advance();
            }
        }
        public int NextIndexAfterWhiteSpace()
        {
            var index = Index;
            while (index < Source.Length && IsWhiteSpace(Source[index]))
            {
                index++;
            }
            return index;
        }

        public static bool IsWhiteSpace(char ch)
        {
            return Char.IsWhiteSpace(ch);
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

        //public abstract IDictionary<string, dynamic> Parse();
    }
}
