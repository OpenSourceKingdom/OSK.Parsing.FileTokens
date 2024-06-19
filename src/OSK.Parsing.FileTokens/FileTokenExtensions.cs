using System;

namespace OSK.Parsing.FileTokens
{
    public static class FileTokenExtensions
    {
        public static string AsCharString(this int value)
            => Convert.ToChar(value).ToString();

        public static char AsChar(this int value)
            => Convert.ToChar(value);
    }
}
