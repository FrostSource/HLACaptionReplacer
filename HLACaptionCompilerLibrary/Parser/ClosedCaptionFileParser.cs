using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    public class ClosedCaptionFileParser : GenericParser
    {
        public override string BoundaryChars { get; set; } = "{}\"";
        public override string CommentBlockStart { get; set; } = "";
        public override string CommentBlockEnd { get; set; } = "";

        public ClosedCaptionFileParser(string source):base(source)
        {
        }
        public ClosedCaptionFileParser(FileInfo file) : this(File.ReadAllText(file.FullName))
        {
        }

        private string NextWordOrString()
        {
            SkipWhiteSpace();
            SavePosition();
            if (CurrentChar == '"')
            {
                return NextEnclosed();
            }
            else
            {
                return NextWord();
            }
        }

        public IDictionary<string, dynamic> Parse()
        {
            var parsed = new Dictionary<string, dynamic>();
            var tokens = new Dictionary<string, string>();

            string word;
            word = NextWordOrString();
            if (word != "lang")
                SyntaxError($"Expecting 'lang' but found {word}", PreviousLineNumber, PreviousLinePosition);

            if (Next() != '{')
                SyntaxError($"Expecting opening brace '{{' but found '{Previous()}'");

            word = NextWordOrString();
            if (word != "Language")
                SyntaxError($"Expecting 'Language' but found {word}", PreviousLineNumber, PreviousLinePosition);

            //TODO: Throw error if non-allowed language?
            var language = NextWordOrString();
            parsed.Add("Language", language);

            word = NextWordOrString();
            if (word != "Tokens")
                SyntaxError($"Expecting 'Tokens' but found {word}", PreviousLineNumber, PreviousLinePosition);

            if (Next() != '{')
                SyntaxError($"Expecting opening brace '{{' but found '{Previous()}'");

            while (Peek() != '}')
            {
                var key = NextWordOrString();
                var value = NextWordOrString();
                tokens.Add(key, value);
            }
            parsed.Add("Tokens", tokens);

            Advance();

            if (Next() != '}')
                SyntaxError($"Expecting closing brace '}}' but found '{Previous()}'");

            return parsed;
        }

        public bool TryParse(out IDictionary<string, dynamic> parsed)
        {
            parsed = null;
            try
            {
                parsed = Parse();
            }
            catch (ParserException e)
            {
                Console.WriteLine(e);
                return false;
            }
            return true;
        }
    }
}
