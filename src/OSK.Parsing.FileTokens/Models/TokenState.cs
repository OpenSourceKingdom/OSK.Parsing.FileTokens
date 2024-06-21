using System;
using System.Linq;

namespace OSK.Parsing.FileTokens.Models
{
    public class TokenState
    {
        #region Variables

        public FileTokenType TokenType { get; }

        public TokenReadState ReadState { get; }

        public int[] Tokens { get; }

        public SingleReadToken? EndToken { get; }

        internal int TokenIndex { get; set; }

        #endregion

        #region Constructors

        public TokenState(FileTokenType tokenType, TokenReadState readState, params int[] tokens)
            : this(tokenType, readState, tokens, null)
        { 
        }

        public TokenState(FileTokenType tokenType, TokenReadState readState, int[] tokens,
            SingleReadToken? endToken)
        {
            TokenType = tokenType;
            ReadState = readState;
            Tokens = tokens ?? throw new ArgumentNullException(nameof(tokens));
            EndToken = endToken;
        }

        #endregion

        #region Helpers

        public bool DoBytesMatch(params int[] value)
        {
            if (value.Length != Tokens.Length)
            {
                return false;
            }

            return Tokens.SequenceEqual(value);
        }

        public FileToken ToFileToken()
            => new FileToken(TokenType, Tokens);

        #endregion
    }
}
