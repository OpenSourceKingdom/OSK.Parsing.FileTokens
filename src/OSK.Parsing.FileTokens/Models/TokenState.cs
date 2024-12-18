using System;
using System.Linq;

namespace OSK.Parsing.FileTokens.Models
{
    /// <summary>
    /// Represents the state of a currently in-process token being read. 
    /// </summary>
    public class TokenState
    {
        #region Variables

        public FileTokenType TokenType { get; }

        public TokenReadState ReadState { get; }

        public int[] Tokens { get; }

        public SingleReadToken? EndToken { get; }

        /// <summary>
        /// This is an internal field for the current implementation of <see cref="Ports.IFileTokenReader"/> to use to keep track of
        /// the current index that a token is being read for <see cref="GenericTokenStateHandler.GetTokenState(int, int)"/>
        /// </summary>
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
