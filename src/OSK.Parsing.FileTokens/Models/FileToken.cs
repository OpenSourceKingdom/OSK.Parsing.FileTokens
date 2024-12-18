using System;
using System.Text;

namespace OSK.Parsing.FileTokens.Models
{
    /// <summary>
    /// An object that represents a token read from a file. The token could be one more characters depending on the token state handler being used.
    /// </summary>
    public class FileToken
    {
        #region Variables

        public FileTokenType TokenType { get; }

        public int[] RawTokens { get; }

        #endregion

        #region Constructors

        public FileToken(FileTokenType tokenType, params int[] rawValue)
        {
            TokenType = tokenType;
            RawTokens = rawValue ?? throw new ArgumentNullException(nameof(rawValue));
        }

        #endregion

        #region Helpers

        public override string ToString()
        {
            if (RawTokens.Length == 0)
            {
                return string.Empty;
            }
            if (RawTokens.Length == 1)
            {
                return RawTokens[0].AsCharString();
            }

            var tokenBuilder = new StringBuilder();
            foreach (var token in RawTokens)
            {
                tokenBuilder.Append(token.AsCharString());
            }

            return tokenBuilder.ToString();
        }

        #endregion
    }
}
