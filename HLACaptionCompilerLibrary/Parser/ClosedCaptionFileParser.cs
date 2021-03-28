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

        private string NextWordOrString(string expecting = "")
        {
            SkipWhiteSpace();
            SavePosition();
            if (CurrentChar == '"')
            {
                return NextEnclosed();
            }
            else
            {
                return NextWord(expecting);
            }
        }

        public IDictionary<string, dynamic> Parse()
        {
            //try
            //{
                var parsed = new Dictionary<string, dynamic>();
                var tokens = new Dictionary<string, string>();

                string word;
                word = NextWordOrString("lang");
                if (word != "lang")
                    SyntaxError($"Expecting 'lang' but found {word}", SavedLineNumber, SavedLinePosition);

                if (Next() != '{')
                    SyntaxError($"Expecting opening brace '{{' but found '{CharToString(Previous())}'");

                word = NextWordOrString("Language");
                if (word != "Language")
                    SyntaxError($"Expecting 'Language' but found {word}", SavedLineNumber, SavedLinePosition);

                //TODO: Throw error if non-allowed language?
                var language = NextWordOrString();
                parsed.Add("Language", language);

                word = NextWordOrString("Tokens");
                if (word != "Tokens")
                    SyntaxError($"Expecting 'Tokens' but found {word}", SavedLineNumber, SavedLinePosition);

                if (Next() != '{')
                    SyntaxError($"Expecting opening brace '{{' but found '{CharToString(Previous())}'");

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
            //}
            //catch (ParserSyntaxException e)
            //{
            //    Console.WriteLine(e);
            //}
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
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}
