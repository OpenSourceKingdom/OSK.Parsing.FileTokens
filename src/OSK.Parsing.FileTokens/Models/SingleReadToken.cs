using System;

namespace OSK.Parsing.FileTokens.Models
{
    public class SingleReadToken
    {
        #region Variables

        public FileTokenType TokenType { get; }

        public int[] Tokens { get; }

        #endregion

        #region Constructors

        public SingleReadToken(FileTokenType tokenType, params int[] startTokens)
        {
            TokenType = tokenType;
            Tokens = startTokens ?? throw new ArgumentNullException(nameof(startTokens));
        }

        #endregion

        #region Helpers

        public bool Matches(SingleReadToken singleToken)
            => Matches(singleToken.Tokens, false);

        public bool Matches(int[] characters, bool allowPartialMatch)
        {
            if (characters.Length > Tokens.Length)
            {
                return false;
            }
            if (!allowPartialMatch && characters.Length < Tokens.Length)
            {
                return false;
            }

            for (var i = 0; i < characters.Length; i++)
            {
                if (characters[i] != Tokens[i])
                {
                    return false;
                }
            }

            return true;
        }

        #endregion
    }
}
