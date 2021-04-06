using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    public class GenericParser
    {
        public List<GenericToken> Tokens { get; internal set; }
        public GenericToken NextToken { get => Tokens[Index]; }
        public int Index { get; private set; }
        public GenericToken CurrentToken
        {
            get
            {
                if (Index >= Tokens.Count) EOFError("unexpected end of file");
                return Tokens[Index];
            }
        }

        protected GenericToken Eat(TokenType tokenType, string value = null, bool caseSensitive = true)
        {
            if (CurrentToken.TokenType != tokenType) SyntaxError(tokenType);
            if (value != null && CurrentToken.Value.ToLower() != (caseSensitive ? value : value.ToLower())) SyntaxError(tokenType, value);
            Index++;
            return Tokens[Index-1];
        }
        protected GenericToken Eat(params TokenType[] tokenTypes)
        {
            foreach (var tokenType in tokenTypes)
            {
                if (CurrentToken.TokenType == tokenType) return CurrentToken;
            }
            SyntaxError(tokenTypes);
            return null;
        }

        /// <summary>
        /// Performs the given delegate one or more times as long as it returns successful.
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        protected bool OneOrMore(Action func)
        {
            var savedIndex = Index;
            // first func out of try because this can fail on zero matches
            func();
            try
            {
                while (true)
                {
                    savedIndex = Index;
                    func();
                }
            }
            catch (ParserSyntaxException)
            {
                Index = savedIndex;
            }
            return true;
        }
        /// <summary>
        /// Performs the given delegate zero or more times as long as it is successful.
        /// </summary>
        /// <param name="func"></param>
        protected void ZeroOrMore(Action func)
        {
            var savedIndex = Index;
            try
            {
                while (true)
                {
                    savedIndex = Index;
                    func();
                }
            }
            catch (ParserSyntaxException)
            {
                Index = savedIndex;
            }
        }
        protected bool EitherOr(params Func<bool>[] funcs)
        {
            var expectedTokenTypes = new List<TokenType>();
            foreach (var func in funcs)
            {
                var savedIndex = Index;
                try
                {
                    var result = func();
                    if (result) return true;
                }
                catch (ParserSyntaxException e)
                {
                    expectedTokenTypes.AddRange(e.ExpectedTokenTypes);
                    Index = savedIndex;
                }
            }
            SyntaxError(expectedTokenTypes.ToArray());
            return false;
        }
        protected void Optional(Action func)
        {
            var savedIndex = Index;
            try
            {
                func();
            }
            catch (ParserSyntaxException)
            {
                Index = savedIndex;
            }
        }

        internal void SyntaxError(TokenType expectedType, string expectedValue = "")
        {
            throw new ParserSyntaxException(expectedType, expectedValue, CurrentToken);
        }
        internal void SyntaxError(TokenType[] expectedTypes)
        {
            throw new ParserSyntaxException(expectedTypes, CurrentToken);
        }
        internal void SyntaxError(string message)
        {
            throw new ParserSyntaxException(message, CurrentToken);
        }
        internal void EOFError(string message)
        {
            throw new ParserEOFException(message);
        }
    }
}
