using OSK.Parsing.FileTokens.Handlers;
using OSK.Parsing.FileTokens.Internal.Services;
using OSK.Parsing.FileTokens.Options;
using OSK.Parsing.FileTokens.Ports;

namespace OSK.Parsing.FileTokens
{
    public static class FileTokenParser
    {
        public static IFileTokenReader OpenRead(string filePath)
            => OpenRead(filePath, DefaultTokenStateHandler.Instance);

        public static IFileTokenReader OpenRead(string filePath, ITokenStateHandler tokenStateHandler)
            => new FileTokenReader(filePath, tokenStateHandler, new FileTokenReaderOptions());
    }
}
