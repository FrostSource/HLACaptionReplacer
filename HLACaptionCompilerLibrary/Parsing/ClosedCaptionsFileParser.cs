using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    public class ClosedCaptionsFileParser : GenericParser
    {
        private ClosedCaptionsFileTokenizer Tokenizer { get; set; }
        private string Source { get; set; }
        private GenericNode NodeTree { get; set; }

        /// <summary>
        /// Gets the language found from the source file after a successful parse.
        /// </summary>
        public string CaptionLanguage { get; private set; } = "";
        /// <summary>
        /// Gets the set of tokens found from the source file after a successful parse.
        /// </summary>
        public Dictionary<string, string> CaptionTokens { get; private set; } = new();

        // Custom properties
        public bool AllowDirectives { get; set; } = true;
        public bool AllowHashRegions { get; set; } = true;
        public bool Strict { get; set; } = true;
        private bool DirectiveInsideFile { get; set; } = false;


        public ClosedCaptionsFileParser(string source, bool strict = false)
        {
            Tokenizer = new ClosedCaptionsFileTokenizer(source);
            Strict = strict;
        }
        public ClosedCaptionsFileParser(FileInfo file, bool strict = false) : this(File.ReadAllText(file.FullName), strict)
        {
        }

        internal GenericToken EatStringOrIdentifier(bool? strictOverride = null)
        {
            strictOverride ??= Strict;
            if (strictOverride.Value)
            {
                return Eat(TokenType.String);
            }
            else
            {
                GenericToken token = null;
                EitherOr(
                    () => (token = Eat(TokenType.String)) != null
                    ,
                    () => (token = Eat(TokenType.Identifier)) != null
                    );
                return token;
            }
        }
        internal GenericToken EatStringOrIdentifier(string value, bool? caseSensitive = null, bool? strictOverride = null)
        {
            caseSensitive ??= Strict;
            strictOverride ??= Strict;
            if (strictOverride.Value)
            {
                return Eat(TokenType.String, value, caseSensitive.Value);
            }
            else
            {
                GenericToken token = null;
                EitherOr(
                    () => (token = Eat(TokenType.String, value, caseSensitive.Value)) != null
                    ,
                    () => (token = Eat(TokenType.Identifier, value, caseSensitive.Value)) != null
                    );
                return token;
            }
        }

        internal string StrictLower(string value)
        {
            if (Strict)
                return value;
            return value.ToLower();
        }

        internal void EatNewLine()
        {
            if (Strict)
            {
                Eat(TokenType.Symbol, "\n");
            }
            else
            {
                Optional(() => Eat(TokenType.Symbol, "\n"));
            }
        }

        /* EBNF style, loosely (strict parsing)
         * 
         * file             = "\n"* , directive* , lang ;
         * lang             = "lang" , "{" , "Language" , string , tokens , "}" ;
         * tokens           = "Tokens" , "{" , (directive|token)+ , "}" ;
         * token            = string , string ;
         * directive        = "#" , identifier , identifier* ;
         * 
         * string           = "\" , { . } , "\"" ;
         * non_whitespace   = { \W } ;
         * 
         * Tree structure:
         * 
         * value = language
         * 
         * left = null
         * right = 
         *      value = tokens list
         *      left = null
         *      right = null
         *      
         * Tokens list:
         * 
         * value = null
         * left =
         *      value = caption token
         *      left = null
         *      right = null
         * right = 
         *      value = caption text
         *      left = null
         *      right = null
         *      
         *
         */

        private GenericNode ParseFile()
        {
            //ZeroOrMore(() => Eat(TokenType.Symbol, "\n"));
            ZeroOrMore(() => ParseDirective());
            return ParseLang();
        }

        private GenericNode ParseLang()
        {
            EatStringOrIdentifier("lang", false, false);
            EatNewLine();
            Eat(TokenType.Symbol, "{");
            EatNewLine();
            EatStringOrIdentifier("Language", false, false);
            PushError($"Language name required {(Strict ? "in quotes" : "")} after \"Language\" key");
            var language = EatStringOrIdentifier().Value.ToLower();
            PopError();
            if (language == "") SyntaxError("Language must not be blank");
            EatNewLine();
            var tokensNode = ParseTokens();
            Eat(TokenType.Symbol, "}");

            return new GenericNode(language, null, tokensNode);
        }

        private GenericNode ParseTokens()
        {
            EatStringOrIdentifier("Tokens", false, false);
            EatNewLine();
            Eat(TokenType.Symbol, "{");
            EatNewLine();
            var tokens = new List<GenericNode>();
            PushError("Expecting directive or caption definition");
            OneOrMore(() =>
                EitherOr(
                    () => ParseDirective()
                    ,
                    () => { tokens.Add(ParseToken()); return true; }
                    )
            );
            PopError();
            Eat(TokenType.Symbol, "}");
            EatNewLine();
            return new GenericNode(tokens, null, null);
        }

        private GenericNode ParseToken()
        {
            // token = string , string ;
            var captionToken = EatStringOrIdentifier(false).Value;
            if (captionToken == "") SyntaxError("Caption token must not be blank");
            var captionText = EatStringOrIdentifier().Value;
            EatNewLine();

            return new GenericNode(null,
                                   new GenericNode(captionToken),
                                   new GenericNode(captionText));
        }

        private bool ParseDirective()
        {
            Eat(TokenType.Symbol, "#");
            string directiveType = Eat(TokenType.Identifier, caseSensitive: false).Value;
            switch (directiveType)
            {
                case "define":
                    Eat(TokenType.Identifier);
                    Eat(TokenType.Identifier);
                    break;

                case "hash-region":
                    throw new ParserException("Directive Error: Hash region directive not yet implemented.");

                default:
                    throw new ParserException($"Directive Error: Unknown directive \"{directiveType}\".");
            }
            Eat(TokenType.Symbol, "\n");
            return true;
        }

        public void Parse()
        {
            Tokens = Tokenizer.Tokenize();
            NodeTree = ParseFile();
            CaptionLanguage = (string)NodeTree.Value;
            foreach (var node in (List<GenericNode>)NodeTree.Right.Value)
            {
                var captionToken = (string)node.Left.Value;
                var captionText = (string)node.Right.Value;
                CaptionTokens.Add(captionToken, captionText);
            }
        }

        public bool TryParse()
        {
            try
            {
                Parse();
                return true;
            }
            catch (ParserException e)
            {
                Console.WriteLine(GenericTokenizer.ReplaceSpecialCharsInString(e.Message));
                return false;
            }
        }
    }
}
