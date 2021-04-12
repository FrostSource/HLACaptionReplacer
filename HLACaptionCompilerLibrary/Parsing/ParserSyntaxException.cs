using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parsing
{
    class ParserSyntaxException : ParserException
    {
        public TokenType[] ExpectedTokenTypes { get; private set; }
        public string[] ExpectedValues { get; private set; }

        public ParserSyntaxException()
        {
        }

        public ParserSyntaxException(TokenType expectedType, GenericToken encounteredToken)
            : base(BuildExceptionMessage(expectedType, encounteredToken))
        {
            ExpectedTokenTypes = new TokenType[] { expectedType };
            ExpectedValues = Array.Empty<string>();
        }
        public ParserSyntaxException(TokenType[] expectedTypes, GenericToken encounteredToken)
            : base(BuildExceptionMessage(expectedTypes, encounteredToken))
        {
            ExpectedTokenTypes = expectedTypes;
            ExpectedValues = Array.Empty<string>();
        }
        public ParserSyntaxException(TokenType expectedType, string expectedValue, GenericToken encounteredToken)
            : base(BuildExceptionMessage(expectedType, expectedValue, encounteredToken))
        {
            ExpectedTokenTypes = new TokenType[] { expectedType };
            ExpectedValues = new string[] { expectedValue };
        }
        public ParserSyntaxException(TokenType[] expectedTypes, string[] expectedValues, GenericToken encounteredToken)
            : base(BuildExceptionMessage(expectedTypes, expectedValues, encounteredToken))
        {
            ExpectedTokenTypes = expectedTypes;
            ExpectedValues = expectedValues;
        }
        public ParserSyntaxException(string message, GenericToken encounteredToken)
            : base(BuildExceptionMessage(message, encounteredToken))
        {
            ExpectedTokenTypes = Array.Empty<TokenType>();
            ExpectedValues = Array.Empty<string>();
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
        private static string BuildExceptionMessage(TokenType expectedType, GenericToken encounteredToken)
        {
            return BuildExceptionMessage($"Expecting {OneOf(expectedType)}", encounteredToken);
        }
        private static string BuildExceptionMessage(TokenType[] expectedTypes, GenericToken encounteredToken)
        {
            return BuildExceptionMessage($"Expecting {OneOf(expectedTypes)}", encounteredToken);
            /*var sb = new StringBuilder($"Expecting one of ({Enum.GetName(typeof(TokenType), expectedTypes[0])}");
            for (var i = 1; i < expectedTypes.Length; i++)
            {
                sb.Append($", {Enum.GetName(typeof(TokenType), expectedTypes[i])}");
            }
            return BuildExceptionMessage(sb.Append($") but found {encounteredToken.TokenType}").ToString(), encounteredToken);*/
        }
        private static string BuildExceptionMessage(TokenType expectedType, string expectedValue, GenericToken encounteredToken)
        {
            return BuildExceptionMessage($"Expecting {OneOf(expectedType)} {WithValue(expectedValue)}", encounteredToken);
            /*var e = Enum.GetName(typeof(TokenType), expectedType);
            var f = Enum.GetName(typeof(TokenType), encounteredToken.TokenType);
            if (expectedValue == "")
            {
                return BuildExceptionMessage($"Expecting {e} but found {f}", encounteredToken);
            }
            else
            {
                return BuildExceptionMessage($"Expecting {e} with value {expectedValue} but found {f} with value {encounteredToken.Value}", encounteredToken);
            }*/
        }
        private static string BuildExceptionMessage(TokenType[] expectedTypes, string[] expectedValues, GenericToken encounteredToken)
        {
            return BuildExceptionMessage($"Expecting {OneOf(expectedTypes)} {WithValue(expectedValues)}", encounteredToken);
        }

        // Helper methods
        private static string OneOf(TokenType expectedType)
        {
            return $"({Enum.GetName(typeof(TokenType), expectedType)})";
        }
        private static string OneOf(TokenType[] expectedTypes)
        {
            if (expectedTypes.Length == 1)
            {
                return OneOf(expectedTypes[0]);
            }

            var sb = new StringBuilder($"one of ({Enum.GetName(typeof(TokenType), expectedTypes[0])}");
            for (var i = 1; i < expectedTypes.Length; i++)
            {
                sb.Append($", {Enum.GetName(typeof(TokenType), expectedTypes[i])}");
            }
            return sb.Append(')').ToString();
        }

        private static string WithValue(string expectedValue)
        {
            if (expectedValue == "")
                return "";

            return $"with value ({expectedValue})";
        }
        private static string WithValue(string[] expectedValues)
        {
            if (expectedValues.Length == 0) return "";
            if (expectedValues.Length == 1) return WithValue(expectedValues[0]);

            var sb = new StringBuilder($"with values ({expectedValues[0]}");
            var count = 0;
            var lastFound = "";
            // for loop because we start at 1
            for (var i = 1; i < expectedValues.Length; i++)
            {
                if (expectedValues[i] != "")
                {
                    count++;
                    lastFound = expectedValues[i];
                    sb.Append($", {expectedValues[i]}");
                }
            }

            if (count == 0) return "";
            if (count == 1) return WithValue(lastFound);

            return sb.Append(')').ToString();
        }
    }
}
