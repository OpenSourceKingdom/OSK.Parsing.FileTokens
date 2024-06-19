namespace OSK.Parsing.FileTokens.Models
{
    public class FileToken
    {
        public FileTokenType TokenType { get; set; }

        public int RawValue { get; set; }

        public string? Value { get; set; }
    }
}
