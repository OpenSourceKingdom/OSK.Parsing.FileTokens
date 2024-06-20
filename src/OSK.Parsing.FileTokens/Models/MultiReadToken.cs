using System;

namespace OSK.Parsing.FileTokens.Models
{
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
