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
        public override string BoundaryChars { get; protected set; } = "{}\"";
        public override string CommentBlockStart { get; protected set; } = "";
        public override string CommentBlockEnd { get; protected set; } = "";
        //public override bool AutomaticallyConvertEscapedCharacters { get; set; } = false;
        public override string InvalidChars { get; protected set; } = "\n";

        // Custom properties
        public bool AllowPreProcessors { get; set; } = true;
        private const char PreProcessorChar = '#';
        public bool AllowHashRegions { get; set; } = true;
        public bool IsStrict { get; set; } = true;


        public ClosedCaptionFileParser(string source, bool strict = false):base(source)
        {
            IsStrict = strict;
        }
        public ClosedCaptionFileParser(FileInfo file, bool strict = false) : this(File.ReadAllText(file.FullName), strict)
        {
        }

        private string NextWordOrString(string expecting = "")
        {
            //SkipWhiteSpace();
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

        public IDictionary<string, dynamic> Parse()
        {
            var parsed = new Dictionary<string, dynamic>();
            var tokens = new Dictionary<string, string>();

            if (AllowPreProcessors)
            {
                //var preprocessorValues = new Dictionary<string, string>();
                while (CurrentChar == PreProcessorChar)
                {
                    Advance();
                    var name = NextWord("pre-processor name", " \t", "\r\n");
                    //var value = NextWord("pre-process value");
                    var value = RestOfLine();
                    SkipGarbage();
                    switch (name)
                    {

                        // Named values
                        default:
                            var line = LineNumber;
                            SetSource(Source[Index..].Replace(name, value));
                            //SkipLine();
                            LineNumber = line;
                            //var index = Index - 1;
                            //SetSource(Source.Replace(name, value));
                            //Advance(index);
                            //preprocessorValues.Add(name, value);
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
                var key = NextWordOrString();
                var value = NextWordOrString();
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
            catch (ParserException e)
            {
                Console.WriteLine(e.Message);
                return false;
            }
            return true;
        }
    }
}
