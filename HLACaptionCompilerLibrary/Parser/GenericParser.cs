using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HLACaptionCompiler.Parser
{
    public class GenericParser
    {
        protected List<GenericToken> Tokens { get; set; }
        protected GenericToken NextToken { get => Tokens[Index]; }
        protected int Index { get; private set; }
        protected Stack<string> ErrorStack { get; set; } = new();
        /// <summary>
        /// Gets the current token, the one waiting to be consumed.
        /// </summary>
        /// <exception cref="ParserEOFException">Thrown when there are no tokens left.</exception>
        protected GenericToken CurrentToken
        {
            get
            {
                if (Tokens.Count == 0 || Index >= Tokens.Count) EOFError();
                return Tokens[Index];
            }
        }
        /// <summary>
        /// Gets the current token, the one waiting to be consumed, if it exists.
        /// This will not throw an exception and returns null if not found.
        /// </summary>
        protected GenericToken SafeCurrentToken
        {
            get
            {
                if (Tokens.Count == 0 || Index >= Tokens.Count) return null;
                return Tokens[Index];
            }
        }

        protected GenericToken Eat(TokenType tokenType, string value = "", bool caseSensitive = true)
        {
            if (CurrentToken.TokenType != tokenType ||
                (value != "" && CurrentToken.Value.ToLower() != (caseSensitive ? value : value.ToLower())))
            {
                SyntaxError(tokenType, value);
            }
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
            catch (ParserException)
            {
                Index = savedIndex;
            }
        }
        protected bool EitherOr(params Func<bool>[] funcs)
        {
            var expectedTokenTypes = new HashSet<TokenType>();//List<TokenType>();
            var expectedValues = new HashSet<string>(); //List<string>();
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
                    expectedTokenTypes.UnionWith(e.ExpectedTokenTypes);
                    expectedValues.UnionWith(e.ExpectedValues);
                    Index = savedIndex;
                }
            }

            SyntaxError(expectedTokenTypes.ToArray(), expectedValues.ToArray());
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

        protected void PushError(string message) => ErrorStack.Push(message);
        protected string PopError()
        {
            if (ErrorStack.Count > 0)
                return ErrorStack.Pop();

            return "";
        }

        internal void SyntaxError(TokenType[] expectedTypes)
        {
            if (ErrorStack.Count > 0) throw new ParserSyntaxException(ErrorStack.Peek(), CurrentToken);
            throw new ParserSyntaxException(expectedTypes, CurrentToken);
        }
        internal void SyntaxError(TokenType expectedType, string expectedValue = "")
        {
            if (ErrorStack.Count > 0) throw new ParserSyntaxException(ErrorStack.Peek(), CurrentToken);
            throw new ParserSyntaxException(expectedType, expectedValue, CurrentToken);
        }
        internal void SyntaxError(TokenType[] expectedTypes, string[] expectedValues)
        {
            if (ErrorStack.Count > 0) throw new ParserSyntaxException(ErrorStack.Peek(), CurrentToken);
            throw new ParserSyntaxException(expectedTypes, expectedValues, CurrentToken);
        }
        internal void SyntaxError(string message)
        {
            if (ErrorStack.Count > 0) throw new ParserSyntaxException(ErrorStack.Peek(), CurrentToken);
            throw new ParserSyntaxException(message, CurrentToken);
        }
        internal void EOFError(string message = "Unexpected end of file")
        {
            if (ErrorStack.Count > 0) throw new ParserSyntaxException(ErrorStack.Peek(), CurrentToken);
            throw new ParserEOFException(message, SafeCurrentToken);
        }
    }
}
