using System;

namespace OSK.Parsing.FileTokens.Models
{
    /// <summary>
    /// Represents a language syntax token that only needs one token to complete interpretation
    /// </summary>
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

        /// <summary>
        /// Checks if the array of characters matches the tokens this read token represents
        /// </summary>
        /// <param name="characters">The characters to compare against the language token</param>
        /// <param name="allowPartialMatch">Whether the match must be exact or partial matches of subarrays are valid. i.e. if we have an array of [1, 2, 3], a partial match is considered valid for subarrays for [1], [1, 2]</param>
        /// <returns></returns>
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
