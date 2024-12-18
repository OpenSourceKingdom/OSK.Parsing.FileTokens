using OSK.Parsing.FileTokens.Handlers;
using OSK.Parsing.FileTokens.Internal.Services;
using OSK.Parsing.FileTokens.Options;
using OSK.Parsing.FileTokens.Ports;

namespace OSK.Parsing.FileTokens
{
    /// <summary>
    /// An accessor utiltiy that will open a file token reader at the given path
    /// </summary>
    public static class FileTokenParser
    {
        /// <summary>
        /// Creates a file token reader targeting the file at file path, using the <see cref="DefaultTokenStateHandler"/>/>
        /// </summary>
        /// <param name="filePath">The path to the file being read</param>
        /// <returns>A file token reader capable of reading file tokens from the file using the default parser</returns>
        public static IFileTokenReader OpenRead(string filePath)
            => OpenRead(filePath, DefaultTokenStateHandler.Instance);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="tokenStateHandler"></param>
        /// <returns></returns>
        public static IFileTokenReader OpenRead(string filePath, ITokenStateHandler tokenStateHandler)
            => new FileTokenReader(filePath, tokenStateHandler, new FileTokenReaderOptions());
    }
}
