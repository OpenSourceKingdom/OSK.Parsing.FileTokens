using OSK.Parsing.FileTokens.Models;
using OSK.Parsing.FileTokens.Ports;
using System;
using System.Linq;

namespace OSK.Parsing.FileTokens
{
    public abstract class GenericTokenStateHandler : ITokenStateHandler
    {
        #region Variables

        private readonly SingleReadToken[] _singleTokens;
        private readonly MultiReadToken[] _multiTokens;

        #endregion

        #region Constructors

        protected GenericTokenStateHandler(SingleReadToken[] singleTokens, MultiReadToken[] multiTokens)
        {
            _singleTokens = singleTokens ?? throw new ArgumentNullException(nameof(singleTokens));
            _multiTokens = multiTokens ?? throw new ArgumentNullException(nameof(multiTokens));
        }

        #endregion

        #region ITokenStateHandler

        public SingleReadToken? GetEndToken(SingleReadToken token)
        {
            if (token is null)
            {
                return null;
            }

            var matchedMultiToken = _multiTokens.FirstOrDefault(multiToken 
                => multiToken.StartToken.Matches(token));
            return matchedMultiToken?.EndToken;
        }

        public TokenState GetInitialTokenState(int character)
        {
            return GetTokenState(character, 0);
        }

        public virtual TokenState GetNextTokenState(TokenState previousState, int character)
        {
            if (previousState == null)
            {
                return GetInitialTokenState(character);
            }
            if (previousState.TokenType == FileTokenType.Text)
            {
                var nextTokenState = GetTokenState(character, 0);
                var stateChanged = nextTokenState.TokenType != FileTokenType.Text;
                var characters = stateChanged
                    ? previousState.Tokens
                    : previousState.Tokens.Append(character).ToArray();
                return stateChanged
                    ? new TokenState(FileTokenType.Text, TokenReadState.EndRead, characters)
                    : new TokenState(FileTokenType.Text, TokenReadState.ReadNext, characters);
            } 
            if (previousState.ReadState == TokenReadState.Reset)
            {
                return GetTokenState(character, previousState.TokenIndex + 1);
            }
            if (previousState.ReadState != TokenReadState.ReadNext)
            {
                return previousState;
            }

            var tokenArray = previousState.Tokens.Append(character).ToArray();
            var singleToken = _singleTokens.FirstOrDefault(token => token.TokenType == previousState.TokenType 
                && token.Matches(tokenArray, true));
            if (singleToken != null)
            {
                return singleToken.Tokens.Length == tokenArray.Length
                    ? new TokenState(singleToken.TokenType, TokenReadState.EndRead, tokenArray)
                    : new TokenState(singleToken.TokenType, TokenReadState.ReadNext, tokenArray)
                    {
                        TokenIndex = previousState.TokenIndex
                    };
            }

            var multiToken = _multiTokens.FirstOrDefault(token => token.StartToken.TokenType == previousState.TokenType
                && token.StartToken.Matches(tokenArray, true));
            if (multiToken != null)
            {
                return multiToken.StartToken.Tokens.Length == tokenArray.Length
                    ? new TokenState(multiToken.StartToken.TokenType, TokenReadState.EndRead,
                        tokenArray, multiToken.EndToken)
                    : new TokenState(multiToken.StartToken.TokenType, TokenReadState.ReadNext,
                        tokenArray, multiToken.EndToken)
                    {
                        TokenIndex = previousState.TokenIndex
                    };
            }

            return new TokenState(FileTokenType.Ignore, TokenReadState.Reset, character);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Gets the token state represented by the byte character read.
        /// </summary>
        /// <param name="character">The byte character read in from the stream</param>
        /// <param name="tokenIndex">
        /// The index of the current path. This is used to allow the file token parser the ability to attempt to continue parsing a token state if a given path fails and there are other potential state options.
        /// For example, if there are a couple of multi-read tokens, with start tokens that look like '/*' and '/+', the token index passed in on the first read would be 0. If the token state returned gives a start
        /// token expectation for '/*' on the first attempt but the actual token is '/*', the file token parser will reset and proceed to the next available multi-read token and pass in 1. In this way, the token index
        /// can help to check all potential token states available for a given token sequence.
        /// </param>
        /// <returns>The <see cref="TokenState"/> represented by the current character read and token index provided</returns>
        protected virtual TokenState GetTokenState(int character, int tokenIndex)
        {
            var tokenArray = new int[] { character };
            var singleToken = _singleTokens
                .Where(token => token.Matches(tokenArray, allowPartialMatch: true))
                .Skip(tokenIndex)
                .FirstOrDefault();
            if (singleToken != null)
            {
                return singleToken.Tokens.Length == 1
                    ? new TokenState(singleToken.TokenType, TokenReadState.SingleRead, character)
                    : new TokenState(singleToken.TokenType, TokenReadState.ReadNext, character)
                    {
                        TokenIndex = tokenIndex
                    };
            }

            var multiToken = _multiTokens
                .Where(token => token.StartToken.Matches(tokenArray, allowPartialMatch: true))
                .Skip(tokenIndex)
                .FirstOrDefault();
            if (multiToken != null)
            {
                return multiToken.StartToken.Tokens.Length == 1
                    ? new TokenState(multiToken.StartToken.TokenType, TokenReadState.SingleRead,
                        tokenArray, multiToken.EndToken)
                    : new TokenState(multiToken.StartToken.TokenType, TokenReadState.ReadNext,
                        tokenArray, multiToken.EndToken)
                    {
                        TokenIndex = tokenIndex
                    };
            }

            return IsValidTextCharacter(character)
                ? new TokenState(FileTokenType.Text, TokenReadState.ReadNext, character)
                : new TokenState(FileTokenType.Ignore, TokenReadState.SingleRead, character);
        }

        // built-in .NET check for ASCII:
        // Per http://www.unicode.org/glossary/#ASCII, ASCII is only U+0000..U+007F.
        protected virtual bool IsValidTextCharacter(int character)
            => (uint)character <= '\x007f';

        #endregion
    }
}
