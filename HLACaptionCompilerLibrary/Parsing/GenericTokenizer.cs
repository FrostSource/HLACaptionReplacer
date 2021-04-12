using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parsing
{
    //TODO: Should extract relevant members into separate GenericLanguageParser class?
    //TODO: Allow characters to stop enclosed, like \n
    public class GenericTokenizer
    {
        public static string AlphaChars { get; protected set; } = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
        public static string DigitChars { get; protected set; } = "1234567890";
        public static string HexChars { get; protected set; } = DigitChars + "abcdefABCDEF";
        /// <summary>
        /// Gets or sets if white space should be skipped by the tokenizer when finding elements such as numbers or words.
        /// </summary>
        public virtual bool AutoSkipGarbage { get; protected set; } = true;
        /// <summary>
        /// Gets or sets the chars that define the default boundary between sequences such as words and numbers.
        /// A sequence will stop when it encounters one of the following: White space, boundary char, EOF.
        /// </summary>
        public virtual string BoundaryChars { get; set; } = "-=+*/{}()[]!;,./%*";
        public virtual string WhiteSpaceCharacters { get; set; } = " \t\r\f\n";
        /// <summary>
        /// Gets or sets the string that indicates the start of a line comment.
        /// </summary>
        public virtual string CommentLineStart { get; set; } = "//";
        public virtual string CommentBlockStart { get; set; } = "/*";
        public virtual string CommentBlockEnd { get; set; } = "*/";
        public virtual string[] Symbols { get; set; } = { "-=", "+=", "*=", "/=", "-", "+", "{", "}", "(", ")", "[", "]", "=", "!", ";", ",", ".", "/", "%", "*" };
        public virtual string StringBoundaryCharacters { get; set; } = "\"";
        public virtual string IdentifierStartCharacters { get; set; } = "_" + AlphaChars;
        public virtual string IdentifierCharacters { get; set; } = "_" + AlphaChars + DigitChars;
        public virtual char DecimalChar { get; set; } = '.';
        /// <summary>
        /// Gets or sets if the tokenizer should not distinguish between <see cref="float"/> and <see cref="int"/>, and instead lump all numbers into <see cref="TokenType.Number"/>.
        /// </summary>
        public virtual bool UseNumberTokenOnly { get; set; } = false;
        /// <summary>
        /// Maps a <see cref="string"/> to custom <see cref="Action"/> functions allowing the tokenizer to defer to it when the <see cref="string"/> is encountered. 
        /// </summary>
        public virtual Dictionary<string, Action<GenericTokenizer>> CustomHandlers { get; set; } = new();
        public virtual List<GenericToken> Tokens { get; private set; } = new();
        public GenericToken LastToken { get; private set; }

        public virtual bool AllowCharacterEscaping { get; set; } = true;
        public virtual bool AutomaticallyConvertEscapedCharacters { get; set; } = true;
        public virtual char EscapeCharacter { get; set; } = '\\';
        /// <summary>
        /// Gets or sets the string of chars that will throw a syntax exception if encountered.
        /// Will not throw exception if encountered during garbage skipping.
        /// </summary>
        public virtual string InvalidChars { get; set; } = "";
        /// <summary>
        /// Gets or sets the string of chars that will throw a syntax exception if NOT encountered.
        /// Set as empty string to ignore.
        /// </summary>
        public virtual string ValidChars { get; set; } = "";

        public string Source { get; protected set; }
        public char CurrentChar
        {
            get
            {
                if (Index < 0 || Index >= Source.Length)
                    return '\0';
                else
                    return Source[Index];
            }
        }
        public char NextChar
        {
            get
            {
                if (Index < -1 || Index >= Source.Length - 1)
                    return '\0';
                else
                    return Source[Index + 1];
            }
        }
        public int Index { get; protected set; } = 0;
        public int LineNumber { get; protected set; } = 1;
        public int LinePosition { get; protected set; } = 1;
        public int PreviousLineNumber { get; protected set; } = 1;
        public int PreviousLinePosition { get; protected set; } = 1;
        public int SavedLineNumber { get; protected set; } = 1;
        public int SavedLinePosition { get; protected set; } = 1;
        public bool EOF { get => Index >= Source.Length; }

        public GenericTokenizer(string source)
        {
            //if (!source.EndsWith('\n'))
            //    source += '\n';

            //Source = source.Trim() + '\n';
            Source = source;
        }

        /// <summary>
        /// Saves the current line number and position for later retrieval.
        /// Usually for more accurate syntax error messages.
        /// </summary>
        public void SavePosition()
        {
            SavedLineNumber = LineNumber;
            SavedLinePosition = LinePosition;
        }
        /// <summary>
        /// Advances the next character in the source text.
        /// </summary>
        /// <exception cref="TokenizerEOFException">Thrown when the end of the source is found instead of a character.</exception>
        public void Advance()
        {
            if (EOF) throw new TokenizerEOFException();
            PreviousLineNumber = LineNumber;
            PreviousLinePosition = LinePosition;
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
        /// <exception cref="TokenizerEOFException">Thrown when the end of the source is found instead of a character.</exception>
        public void Advance(int count)
        {
            for (int i = 0; i < count; i++)
            {
                Advance();
            }
        }
        /// <summary>
        /// Gets the next <see cref="char"/> in the source text and advances, skipping any white space at the beginning.
        /// </summary>
        /// <returns>
        /// The next <see cref="char"/> or \0 if at the end.
        /// </returns>
        /// <exception cref="TokenizerEOFException">Thrown when the end of the source is found instead of a character.</exception>
        public char Next()
        {
            if (AutoSkipGarbage) SkipGarbage();
            if (EOF) return '\0';
            char ch = CurrentChar;
            Advance();
            return ch;
        }
        /// <summary>
        /// Gets the previous <see cref="char"/> in the source text.
        /// </summary>
        /// <returns>The previous <see cref="char"/> or \0 if at the beginning.</returns>
        public char Previous()
        {
            if (Index < 0)
                return '\0';
            return Source[Index - 1];
        }
        /// <summary>
        /// Returns the next <see cref="char"/> in the source text without advancing.
        /// </summary>
        /// <returns></returns>
        public char Peek()
        {
            //var index = Index;
            //TODO: This needs to implement a versin of SkipGarbage without consuming!
            //if (AutoSkipGarbage) index = NextIndexAfterWhiteSpace();
            //if (index >= Source.Length) return '\0';
            //return Source[index];
            var prevLineNumber = LineNumber;
            var prevLinePosition = LinePosition;
            var prevIndex = Index;
            if (AutoSkipGarbage) SkipGarbage();
            char ch = Next();
            LineNumber = prevLineNumber;
            LinePosition = prevLinePosition;
            Index = prevIndex;
            return ch;
        }
        /// <summary>
        /// Checks if <paramref name="str"/> is next in the current position of the source text.
        /// </summary>
        /// <param name="str"></param>
        /// <returns>True if <paramref name="str"/> is next in the current position of the source text.</returns>
        public bool IsNext(string str)
        {
            if (str == "") return false;
            if (str.Length > Source.Length - Index) return false;

            var prevLineNumber = LineNumber;
            var prevLinePosition = LinePosition;
            var prevIndex = Index;
            if (AutoSkipGarbage) SkipGarbage();
            var ret = IsNextNoSkip(str);
            LineNumber = prevLineNumber;
            LinePosition = prevLinePosition;
            Index = prevIndex;
            return ret;
        }
        public bool IsNextNoSkip(string str)
        {
            if (str == "") return false;
            if (str.Length > Source.Length - Index) return false;
            return (Source.Substring(Index, str.Length) == str);
        }
        /// <summary>
        /// Eats the given <see cref="string"/> at the front of the source text.
        /// </summary>
        /// <param name="str"></param>
        /// <exception cref="TokenizerEOFException">Thrown when the tokenizer unexpectedly reaches the end of the source.</exception>
        /// <exception cref="TokenizerSyntaxException">Thrown when <paramref name="str"/> is not next in the source.</exception>
        public void Eat(string str)
        {
            if (str.Length > Source.Length - Index)
            {
                throw new TokenizerEOFException();
            }
            if (!IsNext(str))
            {
                SyntaxError($"Expected '{str}'");
            }
            if (AutoSkipGarbage) SkipGarbage();
            Advance(str.Length);
        }
        /// <summary>
        /// Gets the next non-whitespace string of characters in the source text.
        /// </summary>
        /// <param name="expecting">Optionally specify the word that is expected to provide better error reporting.</param>
        /// <returns></returns>
        public virtual string NextWord(string expecting = "", string boundaryChars = null, string invalidChars = null, string validChars = null, bool failOnEmpty = true, bool allowWhiteSpace = false, bool? allowEscaping = null)
        {
            boundaryChars ??= BoundaryChars;
            invalidChars ??= InvalidChars;
            validChars ??= ValidChars;
            allowEscaping ??= AllowCharacterEscaping;
            expecting = expecting == "" ? "word" : expecting;

            if (AutoSkipGarbage) SkipGarbage();
            var str = new StringBuilder();
            while (!EOF && !boundaryChars.Contains(CurrentChar) && (allowWhiteSpace || !IsWhiteSpace(CurrentChar)))
            {
                if (invalidChars.Contains(CurrentChar)) SyntaxError($"Invalid character \"{CurrentChar}\", expecting {expecting}");
                if (validChars != "" && !validChars.Contains(CurrentChar)) SyntaxError($"Invalid character \"{CurrentChar}\", expecting {expecting}");
                
                string escape;
                if (allowEscaping.Value && (escape = NextEscapeSequence()) != "")
                {
                    if (AutomaticallyConvertEscapedCharacters)
                        str.Append(ConvertStringToEscape(escape));
                    else
                        str.Append(escape);
                }
                else
                {
                    str.Append(CurrentChar);
                    Advance();
                }
            }
            if (failOnEmpty && str.Length == 0)
            {
                SyntaxError($"Expecting {expecting}");
            }
            return str.ToString();
        }
        /// <summary>
        /// Returns the next <see cref="string"/> enclosed between a given <see cref="char"/>.
        /// <para>Makes use of <see cref="EscapeCharacter"/> and <see cref="EscapableStrings"/>.</para>
        /// <para>Does not return the matching <paramref name="boundaryChar"/>.</para>
        /// </summary>
        /// <param name="boundaryChar">The <see cref="char"/> that defines the boundary.</param>
        /// <returns></returns>
        /// <exception cref="TokenizerEOFException">Thrown when the end of the source is found instead of a character.</exception>
        /// <exception cref="TokenizerSyntaxException">Thrown when <paramref name="boundaryChar"/> is not at the beginning or end.</exception>
        public virtual string NextEnclosed(char boundaryChar = '"', string invalidChars = null)
        {
            invalidChars ??= InvalidChars;

            if (AutoSkipGarbage) SkipGarbage();
            if (Next() != boundaryChar)
            {
                SyntaxErrorPrevious($"Expected '{boundaryChar}'");
            }
            // Previous values saved for more accurate error reporting
            // separately from SavePosition so it doesn't override child classes.
            var linePrev = LineNumber;
            var posPrev = LinePosition;

            var enclosedValue = NextWord("", boundaryChar.ToString(), invalidChars, failOnEmpty:false, allowWhiteSpace:true);
            if (EOF || CurrentChar != boundaryChar) SyntaxError($"Missing closing '{boundaryChar}'", linePrev, posPrev);
            Advance();
            return enclosedValue;

            /*var str = new StringBuilder();
            while (CurrentChar != boundaryChar)
            {
                if (EOF) 
                if (InvalidChars.Contains(CurrentChar)) SyntaxError($"Enclosed value ended unexpectedly");
                string escape;
                if ((escape = NextEscapeSequence()) != "")
                {
                    if (AutomaticallyConvertEscapedCharacters)
                        str.Append(ConvertStringToEscape(escape));
                    else
                        str.Append(escape);
                }
                else
                {
                    str.Append(CurrentChar);
                    Advance();
                }
            }
            Advance();
            return str.ToString();*/
        }
        /// <summary>
        /// Gets the next integer in the source text
        /// </summary>
        /// <returns></returns>
        public virtual string NextInteger()
        {
            if (AutoSkipGarbage) SkipGarbage();

            var integer = NextWord("integer", validChars: DigitChars);
            return integer;

            /*var str = new StringBuilder();
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
            return str.ToString();*/
        }
        /// <summary>
        /// Gets the next decimal number in the source text.
        /// </summary>
        /// <remarks>May return a similar result to <see cref="NextInteger"/> if no decimal point is found but a valid number is.</remarks>
        /// <returns></returns>
        public virtual string NextDecimal(char? decimalChar = null)
        {
            decimalChar ??= DecimalChar;

            if (AutoSkipGarbage) SkipGarbage();

            string decimalPre = NextWord("decimal number", BoundaryChars + decimalChar, validChars: DigitChars);
            string decimalPost = "";
            if (CurrentChar == decimalChar)
            {
                decimalPost += decimalChar;
                Advance();
                decimalPost += NextWord("number after decimal", BoundaryChars, validChars: DigitChars, invalidChars:decimalChar.ToString());
            }

            return decimalPre + decimalPost;

            /*var boundaryChars = BoundaryChars;
            int pos;
            if ((pos = BoundaryChars.IndexOf(decimalChar)) != -1)
                boundaryChars = BoundaryChars.Remove(pos, 1);
            var foundDecimalChar = false;
            var str = new StringBuilder();
            while (!EOF && !boundaryChars.Contains(CurrentChar) && !IsWhiteSpace(CurrentChar))
            {
                if (!IsDigit(CurrentChar) && CurrentChar != decimalChar) SyntaxError($"Unexpected character '{CharToString(CurrentChar)}' in number");
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
            return str.ToString();*/
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
        /// var name = tokenizer.NextSequence("_abc","_abc123");
        /// </code>
        /// </example>
        public virtual string NextSequence(string validStartChars, string validChars)
        {
            if (AutoSkipGarbage) SkipWhiteSpace();

            
            if (!validStartChars.Contains(CurrentChar)) SyntaxError($"Expecting one of '{validStartChars}' but found '{SpecialCharToString(CurrentChar)}'");
            string sequence = CurrentChar.ToString();
            Advance();
            sequence += NextWord($"one of '{validStartChars}'", validChars: validChars, failOnEmpty: false);
            return sequence;

            /*var str = new StringBuilder();
            while (!EOF && !IsWhiteSpace(CurrentChar))
            {
                if (!validChars.Contains(CurrentChar)) SyntaxError($"Expecting one of '{validChars}' but found '{CharToString(CurrentChar)}'");
                str.Append(CurrentChar);
                Advance();
            }
            if (str.Length == 0)
            {
                SyntaxError("Expecting sequence");
            }
            return str.ToString();*/
        }
        /// <summary>
        /// Gets the next escape sequence as a string if an escape character is next.
        /// </summary>
        /// <remarks>
        /// This method does not strip any leading garbage by default.
        /// </remarks>
        /// <returns>The next escape sequence with the leading marking character, or blank if not found.</returns>
        public virtual string NextEscapeSequence()
        {
            if (AllowCharacterEscaping && CurrentChar == EscapeCharacter)
            {
                if (IsWhiteSpace(NextChar))
                {
                    SyntaxError("Expecting escape sequence");
                }
                Advance();
                // Single character escapes
                switch (CurrentChar)
                {
                    case '\'': Advance(); return EscapeCharacter + @"'";
                    case '"': Advance(); return EscapeCharacter + @"""";
                    case '\\': Advance(); return EscapeCharacter + @"\";
                    case '0': Advance(); return EscapeCharacter + @"0";
                    case 'a': Advance(); return EscapeCharacter + @"a";
                    case 'b': Advance(); return EscapeCharacter + @"b";
                    case 'f': Advance(); return EscapeCharacter + @"f";
                    case 'n': Advance(); return EscapeCharacter + @"n";
                    case 'r': Advance(); return EscapeCharacter + @"r";
                    case 't': Advance(); return EscapeCharacter + @"t";
                    case 'v': Advance(); return EscapeCharacter + @"v";
                }
                // More complex escapes
                var str = new StringBuilder();
                // Unicode escape sequence for character with hex value xxxx
                if (CurrentChar == 'u')
                {
                    Advance();
                    for (int i = 0; i < 4; i++)
                    {
                        if (!CharIsHexidecimal(CurrentChar))
                        {
                            SyntaxErrorPrevious($"Invalid character in Unicode escape {CurrentChar}");
                        }
                        str.Append(CurrentChar);
                        Advance();
                    }
                    return $"\\u{str}";
                }
                //TODO: \xn[n][n][n] – Unicode escape sequence for character with hex value nnnn (variable length version of \uxxxx)
                //TODO: \Uxxxxxxxx – Unicode escape sequence for character with hex value xxxxxxxx (for generating surrogates)

                SyntaxError("Unrecognized escape sequence");
            }
            return "";
        }
        /// <summary>
        /// Gets all characters up to a given <see cref="char"/>.
        /// <para>This does not skip garbage.</para>
        /// </summary>
        /// <param name="ch"></param>
        /// <returns>The <see cref="string"/> up until the next <paramref name="ch"/> or empty if nothing found. </returns>
        public string NextUntil(char ch)
        {
            var sb = new StringBuilder();
            while (!EOF && CurrentChar != ch)
            {
                sb.Append(CurrentChar);
            }
            return sb.ToString();
        }
        /// <summary>
        /// Gets all characters left on the current line.
        /// </summary>
        /// <returns></returns>
        public string RestOfLine()
        {
            var sb = new StringBuilder();
            while (!EOF && CurrentChar != '\n' && CurrentChar != '\r')
            {
                sb.Append(CurrentChar);
                Advance();
            }
            SkipLine();
            return sb.ToString();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="regex"></param>
        /// <returns></returns>
        public string NextRegex(string regex)
        {
            var rx = new Regex(@"$" + regex);
            var match = rx.Match(Source);
            return match.Value;
        }
        public void SkipGarbage()
        {
            while (IsWhiteSpace(CurrentChar) || IsNextNoSkip(CommentLineStart) || IsNextNoSkip(CommentBlockStart))
            {
                SkipWhiteSpace();
                SkipCommentLine();
                SkipCommentBlock();
            }
        }
        public virtual void SkipCommentLine()
        {
            if (IsNextNoSkip(CommentLineStart)) SkipLine();
        }
        public virtual void SkipCommentBlock()
        {
            if (IsNextNoSkip(CommentBlockStart))
            {
                Advance(CommentBlockStart.Length);
                while (!IsNext(CommentBlockEnd)) Advance();
                Advance(CommentBlockEnd.Length);
            }
        }
        /// <summary>
        /// Advances the source string to the first non-whitespace character.
        /// </summary>
        public virtual void SkipWhiteSpace()
        {
            while (!EOF && IsWhiteSpace(CurrentChar))
            {
                Advance();
            }
        }
        /// <summary>
        /// Gets the index at the end of any current whitespace. That is, the first index with a non-whitespace character.
        /// </summary>
        /// <returns></returns>
        public int NextIndexAfterWhiteSpace()
        {
            var index = Index;
            while (index < Source.Length && IsWhiteSpace(Source[index]))
            {
                index++;
            }
            return index;
        }
        /// <summary>
        /// Moves to the next line in the source if one exists.
        /// </summary>
        public void SkipLine()
        {
            while (!EOF && CurrentChar != '\n')
            {
                Advance();
            }
            if (CurrentChar == '\n') Advance();
        }

        public bool IsWhiteSpace(char ch)
        {
            //return Char.IsWhiteSpace(ch);
            return WhiteSpaceCharacters.Contains(ch);
        }
        public string LinePositionFormatted()
        {
            return $"line {LineNumber}, pos {LinePosition}";
        }
        public static string LinePositionFormatted(int lineNumber, int linePosition)
        {
            return $"line {lineNumber}, pos {linePosition}";
        }
        public static string SpecialCharToString(char ch)
        {
            return ch switch
            {
                '\0' => "null",
                '\r' => "carriage return",
                '\n' => "new line",
                '\t' => "tab",
                '\f' => "form feed",
                '"' => "double quotation mark",
                '\'' => "single quotation mark",
                _ => ch.ToString(),
            };
        }
        public static string ReplaceSpecialCharsInString(string str)
        {
            return str.Replace("\0", "[null]")
                      .Replace("\r", "[carriage return]")
                      .Replace("\n", "[new line]")
                      .Replace("\t", "[tab]")
                      .Replace("\f", "[form feed]");
        }
        public static string ConvertStringToEscape(string escapeString)
        {
            var p = System.Text.RegularExpressions.Regex.Unescape(escapeString);
            return p;
            /*return escapeString switch
            {
                "'" => '\'',
                "\"" => '\"',
                "\\" => '\\',
                "0" => '\0',
                "a" => '\a',
                "b" => '\b',
                "f" => '\f',
                "n" => '\n',
                "r" => '\r',
                "t" => '\t',
                "v" => '\v',
                "xFF" => '\xFF'
            };*/
        }
        public static bool CharIsHexidecimal(char ch)
        {
            return HexChars.IndexOf(ch) > -1;
        }
        public void SetSource(string source)
        {
            Source = source;
            Index = 0;
            LineNumber = 1;
            LinePosition = 1;
            PreviousLineNumber = 1;
            PreviousLinePosition = 1;
            SavedLineNumber = 1;
            SavedLinePosition = 1;
        }
        public void SyntaxError(string message, int lineNumber, int linePosition)
        {
            throw new TokenizerSyntaxException(message, lineNumber, linePosition);
        }
        public void SyntaxError(string message)
        {
            SyntaxError(message, LineNumber, LinePosition);
        }
        public void SyntaxErrorPrevious(string message)
        {
            SyntaxError(message, PreviousLineNumber, PreviousLinePosition);
        }
        public void SyntaxErrorSaved(string message)
        {
            SyntaxError(message, SavedLineNumber, SavedLinePosition);
        }

        public static bool IsDigit(char ch)
        {
            return (ch == '0' || ch == '1' || ch == '2' || ch == '3' || ch == '4' || ch == '5' || ch == '6' || ch == '7' || ch == '8' || ch == '9');
        }

        protected virtual void AddToken(TokenType tokenType, string value)
        {
            var token = new GenericToken(tokenType, value, Index, SavedLineNumber, SavedLinePosition);
            Tokens.Add(token);
            LastToken = token;
        }

        public virtual void TokenizeNext()
        {
            SavePosition();
            foreach (var handler in CustomHandlers)
            {
                if (IsNextNoSkip(handler.Key))
                {
                    handler.Value(this);
                    return;
                }
            }

            if (IdentifierStartCharacters.Contains(CurrentChar))
            {
                var value = NextWord("identifier", validChars: IdentifierCharacters, allowEscaping: false);
                AddToken(TokenType.Identifier, value);
                return;
            }

            // Integer or float
            if (DigitChars.Contains(CurrentChar))
            {
                var value = NextDecimal();
                if (UseNumberTokenOnly)
                {
                    AddToken(TokenType.Number, value);
                }
                else
                {
                    if (value.Contains(DecimalChar))
                        AddToken(TokenType.Float, value);
                    else
                        AddToken(TokenType.Integer, value);
                }
                
                return;
            }

            if (StringBoundaryCharacters.Contains(CurrentChar))
            {
                var value = NextEnclosed(CurrentChar);
                AddToken(TokenType.String, value);
                return;
            }

            foreach (string symbol in Symbols)
            {
                if (IsNext(symbol))
                {
                    Advance(symbol.Length);
                    AddToken(TokenType.Symbol, symbol);
                    return;
                }
            }

            SyntaxError($"unknown character \"{CurrentChar}\"");
        }

        public virtual List<GenericToken> Tokenize()
        {
            while (!EOF)
            {
                if (AutoSkipGarbage) SkipGarbage();

                TokenizeNext();
            }
            return Tokens;
        }

        //public abstract IDictionary<string, dynamic> Parse();
    }
}
