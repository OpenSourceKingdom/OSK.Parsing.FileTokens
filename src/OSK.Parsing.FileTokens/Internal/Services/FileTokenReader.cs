using OSK.Parsing.FileTokens.Models;
using OSK.Parsing.FileTokens.Options;
using OSK.Parsing.FileTokens.Ports;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Parsing.FileTokens.Internal.Services
{
    internal class FileTokenReader : IFileTokenReader
    {
        #region Variables

        private readonly ITokenStateHandler _tokenStateHandler;
        private readonly FileTokenReaderOptions _options;

        internal readonly FileStream _fileStream;

        #endregion

        #region Constructors

        public FileTokenReader(string filePath, ITokenStateHandler tokenStateHandler,
            FileTokenReaderOptions options)
            : this (File.OpenRead(filePath), tokenStateHandler, options)
        {
        }

        internal FileTokenReader(FileStream fileStream, ITokenStateHandler tokenStateHandler, FileTokenReaderOptions options)
        {
            _tokenStateHandler = tokenStateHandler ?? throw new ArgumentNullException(nameof(tokenStateHandler));
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _fileStream = fileStream ?? throw new ArgumentNullException(nameof(fileStream));
        }

        #endregion

        #region IFileTokenReader

        public string FilePath => _fileStream.Name;

        public async ValueTask<FileToken> ReadToEndTokenAsync(FileToken fileToken, CancellationToken cancellationToken = default)
        {
            if (fileToken == null)
            {
                throw new ArgumentNullException(nameof(fileToken));
            }
            if (_fileStream.Length == _fileStream.Position)
            {
                return new FileToken(FileTokenType.EndOfFile, -1);
            }

            var endToken = _tokenStateHandler.GetEndToken(new SingleReadToken(fileToken.TokenType, fileToken.RawTokens));
            if (endToken == null)
            {
                return fileToken;
            }

            return await ReadToBytesAsync(fileToken.TokenType, fileToken.RawTokens.ToList(),
                endToken.Tokens, cancellationToken);
        }

        public async ValueTask<FileToken> ReadTokenAsync(CancellationToken cancellationToken = default)
        {
            if (_fileStream.Length == _fileStream.Position)
            {
                return new FileToken(FileTokenType.EndOfFile, -1);
            }

            var tokenState = await GetNextTokenAsync(cancellationToken);
            if (tokenState.EndToken == null)
            {
                return tokenState.ToFileToken();
            }

            return await ReadToBytesAsync(tokenState.TokenType, tokenState.Tokens.ToList(), 
                tokenState.EndToken.Tokens, cancellationToken);
        }

        public void Dispose()
        {
            _fileStream.Dispose();
        }

        #endregion

        #region Helpers

        private async ValueTask<TokenState> GetNextTokenAsync(CancellationToken cancellationToken)
        {
            var iterationsUntilYield = _options.IterationsUntilYield;
            TokenState? previousTokenState = null;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (iterationsUntilYield <= 0)
                {
                    await Task.Yield();
                    iterationsUntilYield = _options.IterationsUntilYield;
                }

                var fileByte = _fileStream!.ReadByte();
                iterationsUntilYield--;

                var tokenState = previousTokenState == null
                    ? _tokenStateHandler.GetInitialTokenState(fileByte)
                    : _tokenStateHandler.GetNextTokenState(previousTokenState, fileByte);

                switch (tokenState.ReadState)
                {
                    case TokenReadState.SingleRead:
                    case TokenReadState.EndRead:
                        if (tokenState.TokenType == FileTokenType.Ignore)
                        {
                            continue;
                        }
                        if (tokenState.TokenType == FileTokenType.Text)
                        {
                            _fileStream.Position--;
                        }
                        return tokenState;
                    case TokenReadState.ReadNext:
                        previousTokenState = tokenState;
                        break;
                    case TokenReadState.Reset:
                        if (previousTokenState == null)
                        {
                            throw new InvalidOperationException("Unable to reset the token read state when no previous state has been read.");
                        }
                        _fileStream.Position = _fileStream.Position - previousTokenState.Tokens.Length - 1;
                        break;
                }
            }
        }

        private async ValueTask<FileToken> ReadToBytesAsync(FileTokenType tokenType, List<int> tokenBuilder, int[] expectedBytes, CancellationToken cancellationToken)
        {
            var currentMatchingIndex = 0;
            var iterationsUntilYield = _options.IterationsUntilYield;
            while (_fileStream!.Position != _fileStream.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileByte = _fileStream.ReadByte();
                tokenBuilder.Add(fileByte);

                if (expectedBytes[currentMatchingIndex] == fileByte)
                {
                    currentMatchingIndex++;
                    if (currentMatchingIndex >= expectedBytes.Length)
                    {
                        return new FileToken(tokenType, tokenBuilder.ToArray());
                    }
                }
                else
                {
                    currentMatchingIndex = 0;
                }

                iterationsUntilYield--;
                if (iterationsUntilYield <= 0)
                {
                    await Task.Yield();
                    iterationsUntilYield = _options.IterationsUntilYield;
                }
            }

            throw new InvalidOperationException($"Unable to read tokens in the given file due to unexpected formatting with a file token of type {tokenType}.");
        }

        #endregion
    }
}
