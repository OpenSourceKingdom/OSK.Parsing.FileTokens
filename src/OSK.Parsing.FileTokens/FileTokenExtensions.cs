using System;

namespace OSK.Parsing.FileTokens
{
    public static class FileTokenExtensions
    {
        /// <summary>
        /// Converts the file token into its string equivalent
        /// </summary>
        /// <param name="value">The raw int value for the file token read from the stream</param>
        /// <returns>The character string of the token</returns>
        public static string AsCharString(this int value)
            => Convert.ToChar(value).ToString();
    }
}
