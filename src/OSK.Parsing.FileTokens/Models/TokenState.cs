using System;

namespace OSK.Parsing.FileTokens.Models
{
    public class TokenState
    {
        public FileTokenType TokenType { get; set; }

        public TokenReadState ReadState { get; set; }

        public int Token { get; set; }

        public int[]? ReadToBytes { get; set; }

        public FileToken ToFileToken()
            => new FileToken()
            {
                TokenType = TokenType,
                Value = GetTokenValue(),
            };

        private string? GetTokenValue()
            => TokenType switch
            {
                FileTokenType.EndOfFile => null,
                FileTokenType.NewLine => Environment.NewLine,
                _ => Token.AsCharString(),
            };
    }
}
