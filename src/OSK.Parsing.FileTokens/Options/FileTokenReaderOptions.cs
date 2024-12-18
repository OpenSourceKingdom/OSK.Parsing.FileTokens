namespace OSK.Parsing.FileTokens.Options
{
    public class FileTokenReaderOptions
    {
        /// <summary>
        /// Represents the number of characters a token reader will consume before yielding operation to other processes
        /// </summary>
        public int IterationsUntilYield { get; set; } = 50;
    }
}
