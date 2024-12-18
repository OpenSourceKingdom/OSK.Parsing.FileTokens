using System;

namespace OSK.Parsing.FileTokens.Models
{
    /// <summary>
    /// Represents a language syntax token that has a start and end state and can require multiple token reads to complete. For example, in C#, a
    /// multi read token could be seen as a multi-line comment in the form of a start token with `/*` and an end token with `*/`
    /// </summary>
    public class MultiReadToken
    {
        #region Variables

        public SingleReadToken StartToken { get; }

        public SingleReadToken EndToken { get; }

        #endregion

        #region Constructors

        public MultiReadToken(SingleReadToken startToken, SingleReadToken endToken)
        {
            StartToken = startToken ?? throw new ArgumentNullException(nameof(startToken));
            EndToken = endToken ?? throw new ArgumentNullException(nameof(endToken));
        }

        #endregion
    }
}
