using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parsing
{
    public class CaptionTextTokenizer : GenericTokenizer
    {
        public override string InvalidChars { get; set; } = "\r\n";
        public override string CommentLineStart { get; set; } = "";
        public override string CommentBlockStart { get; set; } = "";
        public override string CommentBlockEnd { get; set; } = "";
        public override string BoundaryChars { get; set; } = "<>:,";
        public override string WhiteSpaceCharacters { get; set; } = "";
        public override bool AutoSkipGarbage { get; protected set; } = false;
        public override string[] Symbols { get; set; } = Array.Empty<string>();
        public override string StringBoundaryCharacters { get; set; } = "";

        private bool InsideCaptionCode { get; set; } = false;
        public CaptionTextTokenizer(string source) : base(source)
        {
        }

        public override void TokenizeNext()
        {
            switch (CurrentChar)
            {
                case '<':
                    InsideCaptionCode = true;
                    goto case ',';
                case '>':
                    InsideCaptionCode = false;
                    goto case ',';
                case ':':
                case ',':
                    AddToken(TokenType.Symbol, CurrentChar.ToString());
                    Advance();
                    break;

                default:
                    // Capturing plain text
                    if (!InsideCaptionCode)
                    {
                        string text = NextWord("caption text", "<", failOnEmpty: false, allowWhiteSpace: true);
                        AddToken(TokenType.Identifier, text);
                        break;
                    }
                    // Capturing special text inside codes
                    //text = NextWord("caption code text", failOnEmpty: false);
                    base.TokenizeNext();
                    break;
            }
            
        }
    }
}
