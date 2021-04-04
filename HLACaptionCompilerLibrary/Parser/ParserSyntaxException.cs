using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    class ParserSyntaxException : ParserException
    {
        public TokenType[] ExpectedTokenTypes { get; private set; }
        public ParserSyntaxException()
        {
        }

        public ParserSyntaxException(TokenType expectedType, string expectedValue, GenericToken encounteredToken)
            : base(BuildExceptionMessage(expectedType, expectedValue, encounteredToken))
        {
            ExpectedTokenTypes = new TokenType[] { expectedType };
        }
        public ParserSyntaxException(TokenType[] expectedTypes, GenericToken encounteredToken)
            : base(BuildExceptionMessage(expectedTypes, encounteredToken))
        {
            ExpectedTokenTypes = expectedTypes;
        }
        //public ParserException(string message, GenericToken encounteredToken)
        //    : base(BuildExceptionMessage(message, encounteredToken))
        //{
        //}

        public ParserSyntaxException(string message, Exception inner)
            : base(message, inner)
        {
        }

        private static string BuildExceptionMessage(string message, GenericToken encounteredToken)
        {
            return $"{message} at line {encounteredToken.LineNumber}, pos {encounteredToken.LinePosition}";
        }
        private static string BuildExceptionMessage(TokenType expectedType, string expectedValue, GenericToken encounteredToken)
        {
            var e = Enum.GetName(typeof(TokenType), expectedType);
            var f = Enum.GetName(typeof(TokenType), encounteredToken.TokenType);
            if (expectedValue == "")
            {
                return BuildExceptionMessage($"Expecting {e} but found {f}", encounteredToken);
            }
            else
            {
                return BuildExceptionMessage($"Expecting {e} with value {expectedValue} but found {f} with value {encounteredToken.Value}", encounteredToken);
            }
        }
        private static string BuildExceptionMessage(TokenType[] expectedTypes, GenericToken encounteredToken)
        {
            var sb = new StringBuilder($"Expecting one of ({Enum.GetName(typeof(TokenType), expectedTypes[0])}");
            for (var i = 1; i < expectedTypes.Length; i++)
            {
                sb.Append($", {Enum.GetName(typeof(TokenType), expectedTypes[i])}");
            }
            return BuildExceptionMessage(sb.Append($") but found {encounteredToken.TokenType}").ToString(), encounteredToken);
        }
    }
}
