using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    public class ClosedCaptionsFileTokenizer : GenericTokenizer
    {
        private Dictionary<int, (string preDirectiveIndex, string preDirectiveLineNumber, string preDirectiveLinePosition)> PreDirectives { get; set; } = new();

        public override string CommentLineStart { get; set; } = "//";
        public override string CommentBlockStart { get; set; } = "";
        public override string CommentBlockEnd { get; set; } = "";
        public override string WhiteSpaceCharacters { get; set; } = " \t\r\f";
        public override string BoundaryChars { get; set; } = "{}\n\"";
        public override string[] Symbols { get; set; } = { "{", "}", "\n", "#" };
        public override string StringBoundaryCharacters { get; set; } = "\"";
        public override string IdentifierStartCharacters { get; set; } = "_" + AlphaChars;
        public override string IdentifierCharacters { get; set; } = "_!." + AlphaChars + DigitChars;

        private char DirectiveChar { get; set; } = '#';
        private Dictionary<string, string> DirectiveDefineSet { get; set; } = new();
        private bool CatchNewLine { get; set; } = false;

        public ClosedCaptionsFileTokenizer(string source) : base(source)
        {
            /*CustomHandlers.Add("#", t =>
            {
                Advance();
                var directiveType = NextWord("directive type", " \t\r\n");
                switch (directiveType)
                {
                    case "define":
                        var defineName = NextWord("directive define name", "");
                        var defineValue = RestOfLine();
                        SkipLine();
                        var indexAfterDirective = Index;
                        var nextPos = Index;
                        var newSource = new StringBuilder(Source);
                        while ((nextPos = Source.(,defineName, nextPos)) > -1)
                        {
                            newSource.
                        }
                        SetSource(.Replace(defineName, defineValue));
                        // Advance the line number artificially.
                        LineNumber = line;
                }
            });*/
        }

        protected override void AddToken(TokenType tokenType, string value)
        {
            // Don't catch newlines if we've previous had a newline, empty lines should not be tokenized
            if (tokenType == TokenType.Symbol && value == "\n")
            {
                if (CatchNewLine)
                    CatchNewLine = false;
                else
                    return;
            }
            else
            {
                CatchNewLine = true;
            }

            foreach (var defineDirective in DirectiveDefineSet)
            {
                value = value.Replace(defineDirective.Key, defineDirective.Value);
            }
            base.AddToken(tokenType, value);
        }

        public override void TokenizeNext()
        {
            if (CurrentChar == DirectiveChar)
            {
                
                Advance();
                AddToken(TokenType.Symbol, DirectiveChar.ToString());
                var directiveType = NextWord("directive type");
                AddToken(TokenType.Identifier, directiveType);
                switch (directiveType)
                {
                    case "define":
                        var defineName = NextWord("define directive name");
                        var defineValue = NextWord("define directive value");
                        AddToken(TokenType.Identifier, defineName);
                        AddToken(TokenType.Identifier, defineValue);
                        DirectiveDefineSet.Add(defineName, LastToken.Value);
                        break;

                    default:
                        while (CurrentChar != '\r' && CurrentChar != '\n')
                        {
                            var directiveIdentifier = NextWord("directive identifier", " \t\r\n");
                            AddToken(TokenType.Identifier, directiveIdentifier);
                        }
                        break;
                }
                //SkipLine();
                return;
            }

            base.TokenizeNext();
        }
    }
}
