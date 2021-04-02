using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    public class ClosedCaptionFileParser : GenericTokenizer
    {
        public override string BoundaryChars { get; protected set; } = "{}\"";
        public override string CommentBlockStart { get; protected set; } = "";
        public override string CommentBlockEnd { get; protected set; } = "";
        //public override bool AutomaticallyConvertEscapedCharacters { get; set; } = false;
        public override string InvalidChars { get; protected set; } = "\n";

        /// <summary>
        /// Gets the language found from the source file after a successful parse.
        /// </summary>
        public string Language { get; private set; } = "";
        /// <summary>
        /// Gets the set of tokens found from the source file after a successful parse.
        /// </summary>
        public Dictionary<string, string> Tokens { get; private set; } = new();

        // Custom properties
        public bool AllowDirectives { get; set; } = true;
        private const char DirectiveChar = '#';
        public bool AllowHashRegions { get; set; } = true;
        public bool IsStrict { get; set; } = true;
        private bool DirectiveInsideFile { get; set; } = false;


        public ClosedCaptionFileParser(string source, bool strict = false):base(source)
        {
            IsStrict = strict;
        }
        public ClosedCaptionFileParser(FileInfo file, bool strict = false) : this(File.ReadAllText(file.FullName), strict)
        {
        }

        private string NextWordOrString(string expecting = "")
        {
            SkipGarbage();
            SavePosition();
            if (IsStrict)
            {
                 return NextEnclosed();
            }
            else
            {
                if (Peek() == '"')
                {
                    return NextEnclosed();
                }
                else
                {
                    return NextWord(expecting);
                }
            }
        }

        private void EatWordOrString(string expecting = "")
        {
            string word;
            if ((word = NextWordOrString(expecting)) != expecting)
            {
                SyntaxErrorSaved($"Expecting '{expecting}' but found {word}");
            }
        }

        /* EBNF style, loosely (strict parsing)
         * 
         * file             = { directive } , lang ;
         * lang             = "lang" , "{" , "Language" , string , tokens , "}" ;
         * tokens           = "Tokens" , "{" , { directive | token } , "}" ;
         * token            = string , string ;
         * directive        = "#" , non_whitespace , [ { . } ] ;
         * 
         * string           = "\" , { . } , "\"" ;
         * non_whitespace   = { \W } ;
         *
         */

        private void ParseFile()
        {
            // file = { directive } , lang ;
            var foundDirective = ParseDirective();
            while (foundDirective)
            {
                foundDirective = ParseDirective();
            }
            ParseLang();
        }

        private void ParseLang()
        {
            // lang = "lang" , "{" , "Language" , string , tokens , "}" ;
            EatWordOrString("lang");
            Eat("{");
            EatWordOrString("Language");
            var language = NextWordOrString("language name");
            var tokens = ParseTokens();
            Eat("}");
            Language = language;
            Tokens = tokens;
        }

        private Dictionary<string, string> ParseTokens()
        {
            //tokens = "Tokens" , "{" , { directive | token } , "}" ;
            EatWordOrString("Tokens");
            Eat("{");
            var tokens = new Dictionary<string, string>();
            while (CurrentChar != '}')
            {

                // NextWordOrString uses SavePosition so we save manually.
                var savedLineNumber = LineNumber;
                var savedLinePosition = LinePosition;
                (string token, string value) = ParseToken();
                if (tokens.ContainsKey(token))
                {
                    if (IsStrict)
                    {
                        SyntaxError($"Duplicate key found for {token}", savedLineNumber, savedLinePosition);
                    }
                    continue;
                }
                tokens.Add(token, value);
            }
            Eat("}");
            return tokens;
        }

        private (string token, string value) ParseToken()
        {
            // token = string , string ;
            var token = NextWordOrString();
            var value = NextWordOrString();
            return (token, value);
        }

        private bool ParseDirective()
        {
            // directive = "#" , non_whitespace , [ { . } ] ;
            if (AllowDirectives)
            {
                if (CurrentChar == DirectiveChar)
                {
                    Advance();
                    var directiveType = NextWord("directive type", " \t", "\r\n");
                    switch (directiveType)
                    {

                        // Named values
                        case "define":
                            var macroName = NextWord("macro name", " \t", "\r\n");
                            SkipWhiteSpace();
                            var macroValue = RestOfLine();
                            SkipLine();
                            var line = LineNumber;
                            SetSource(Source[Index..].Replace(macroName, macroValue));
                            // Advance the line number artificially.
                            LineNumber = line;
                            return true;

                        default:
                            SyntaxError($"Unknown directive \"{directiveType}\"");
                            break;
                    }
                }
            }
            return false;
        }


        public IDictionary<string, dynamic> Parse()
        {
            var parsed = new Dictionary<string, dynamic>();
            var tokens = new Dictionary<string, string>();

            if (AllowDirectives)
            {
                //var preprocessorValues = new Dictionary<string, string>();
                while (CurrentChar == DirectiveChar)
                {
                    Advance();
                    var directiveType = NextWord("directive type", " \t", "\r\n");
                    switch (directiveType)
                    {

                        // Named values
                        case "define":
                            var macroName = NextWord("macro name", " \t", "\r\n");
                            SkipWhiteSpace();
                            var macroValue = RestOfLine();
                            SkipLine();
                            var line = LineNumber;
                            SetSource(Source[Index..].Replace(macroName, macroValue));
                            // Advance the line number artificially.
                            LineNumber = line;
                            break;

                        default:
                            SyntaxError($"Unknown directive \"{directiveType}\"");
                            break;
                    }
                }
            }

            string word;
            word = NextWordOrString("lang");
            if (word != "lang")
                SyntaxErrorSaved($"Expecting 'lang' but found {word}");

            if (Next() != '{')
                SyntaxErrorPrevious($"Expecting opening brace '{{' but found '{CharToString(Previous())}'");

            word = NextWordOrString("Language");
            if (word != "Language")
            SyntaxErrorSaved($"Expecting 'Language' but found {word}");

            //TODO: Throw error if non-allowed language?
            var language = NextWordOrString();
            parsed.Add("Language", language);

            word = NextWordOrString("Tokens");
            if (word != "Tokens")
            SyntaxErrorSaved($"Expecting 'Tokens' but found {word}");

            if (Next() != '{')
                SyntaxErrorPrevious($"Expecting opening brace '{{' but found '{CharToString(Previous())}'");

            while (Peek() != '}')
            {
                // NextWordOrString uses SavePosition so we save manually.
                var savedLineNumber = LineNumber;
                var savedLinePosition = LinePosition;
                var key = NextWordOrString();
                var value = NextWordOrString();
                if (tokens.ContainsKey(key))
                {
                    if (IsStrict)
                    {
                        SyntaxError($"Duplicate key found for {key}", savedLineNumber, savedLinePosition);
                    }
                    continue;
                }
                tokens.Add(key, value);
            }
            parsed.Add("Tokens", tokens);

            Advance();

            if (Next() != '}')
                SyntaxErrorPrevious($"Expecting closing brace '}}' but found '{Previous()}'");

            return parsed;
        }

        public bool TryParse(out IDictionary<string, dynamic> parsed)
        {
            parsed = null;
            try
            {
                parsed = Parse();
            }
            catch (TokenizerException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}
