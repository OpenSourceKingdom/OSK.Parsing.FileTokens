using OSK.Parsing.FileTokens.Models;
using OSK.Parsing.FileTokens.Options;
using OSK.Parsing.FileTokens.Ports;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Parsing.FileTokens.Internal.Services
{
    internal class FileTokenReader : IFileTokenReader
    {
        #region Variables

        private readonly string _filePath;
        private readonly FileStream _fileStream;
        private readonly ITokenStateHandler _tokenStateHandler;
        private readonly FileTokenReaderOptions _options;

        #endregion

        #region Constructors

        public FileTokenReader(string filePath, ITokenStateHandler tokenStateHandler,
            FileTokenReaderOptions options)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentException(nameof(filePath));
            }
            _filePath = filePath;
            _tokenStateHandler = tokenStateHandler ?? throw new ArgumentNullException(nameof(tokenStateHandler));
            _options = options ?? throw new ArgumentNullException(nameof(options));

            _fileStream = File.OpenRead(filePath);
        }

        #endregion

        #region IFileTokenReader

        public string FilePath => _filePath;

        public async ValueTask<FileToken> ReadToFileTokenEndValueAsync(FileToken fileToken, CancellationToken cancellationToken = default)
        {
            if (fileToken == null)
            {
                throw new ArgumentNullException(nameof(fileToken));
            }
            if (fileToken.TokenType != FileTokenType.ClosureStart)
            {
                return fileToken;
            }
            var targetTokenValue = _tokenStateHandler.GetTokenEndValue(Convert.ToInt16(fileToken.Value)).GetValueOrDefault(-1);
            if (targetTokenValue == -1)
            {
                return fileToken;
            }

            var iterationsUntilYied = _options.IterationsUntilYield;
            var identicalFileTokensDiscovered = 0;
            while (true)
            {
                var fileByte = _fileStream!.ReadByte();
                var tokenState = _tokenStateHandler.GetTokenState(fileByte);
                if (tokenState.TokenType == FileTokenType.EndOfFile)
                {
                    throw new InvalidOperationException("Was not able to read to the file token's end value before reaching the end of the file.");
                }

                if (tokenState.TokenType == fileToken.TokenType)
                {
                    identicalFileTokensDiscovered++;
                }
                else if (tokenState.Token == targetTokenValue)
                {
                    identicalFileTokensDiscovered--;
                    if (identicalFileTokensDiscovered <= 0)
                    {
                        return tokenState.ToFileToken();
                    }
                }

                iterationsUntilYied--;
                if (iterationsUntilYied <= 0)
                {
                    await Task.Yield();
                    iterationsUntilYied = _options.IterationsUntilYield;
                }
            }
        }

        public async ValueTask<FileToken> ReadTokenAsync(CancellationToken cancellationToken = default)
        {
            if (_fileStream == null)
            {
                throw new InvalidOperationException($"Unable to parse a file into tokens when the underlyng file stream has been closed.");
            }
            if (_fileStream.Length == _fileStream.Position)
            {
                return new FileToken()
                {
                    TokenType = FileTokenType.EndOfFile
                };
            }

            var tokenState = await GetNextSingleTokenAsync(cancellationToken);
            return tokenState.ReadState switch
            {
                TokenReadState.Single => tokenState.ToFileToken(),
                TokenReadState.Multiple => await ReadMultiStateTokenAsync(tokenState, cancellationToken),
                _ => throw new InvalidOperationException($"The current read state {tokenState.ReadState} does not have a known conversion to a file token.")
            };
        }

        public void Dispose()
        {
            _fileStream?.Dispose();
        }

        #endregion

        #region Helpers

        private async ValueTask<TokenState> GetNextSingleTokenAsync(CancellationToken cancellationToken)
        {
            var iterationsUntilYield = _options.IterationsUntilYield;
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileByte = _fileStream!.ReadByte();
                var tokenState = _tokenStateHandler.GetTokenState(fileByte);
                if (tokenState.TokenType != FileTokenType.Ignore)
                {
                    return tokenState;
                }

                iterationsUntilYield--;
                if (iterationsUntilYield <= 0)
                {
                    await Task.Yield();
                    iterationsUntilYield = _options.IterationsUntilYield;
                }
            }
        }

        private async ValueTask<FileToken> ReadMultiStateTokenAsync(TokenState tokenState, CancellationToken cancellationToken)
        {
            var tokenBuilder = new StringBuilder();
            tokenBuilder.Append(tokenState.Token.AsCharString());

            if (tokenState.TokenType == FileTokenType.Text)
            {
                var iterationsUntilYield = _options.IterationsUntilYield;
                while (true)
                {
                    var fileByte = _fileStream!.ReadByte();
                    tokenState = _tokenStateHandler.GetTokenState(fileByte);
                    if (tokenState.TokenType != FileTokenType.Text)
                    {
                        break;
                    }

                    tokenBuilder.Append(tokenState.Token.AsCharString());
                    iterationsUntilYield--;
                    if (iterationsUntilYield <= 0)
                    {
                        await Task.Yield();
                        iterationsUntilYield = _options.IterationsUntilYield;
                    }
                }

                _fileStream!.Position--;
                return new FileToken()
                {
                    TokenType = FileTokenType.Text,
                    Value = tokenBuilder.ToString(),
                };
            }

            var nextFileByte = _fileStream!.ReadByte();
            var nextTokenState = _tokenStateHandler.GetTokenState(tokenState, nextFileByte);

            switch (nextTokenState.ReadState)
            {
                case TokenReadState.Reset:
                    _fileStream.Position--;
                    return tokenState.ToFileToken();
                case TokenReadState.Single:
                    tokenBuilder.Append(nextTokenState.Token.AsCharString());
                    return new FileToken()
                    {
                        TokenType = nextTokenState.TokenType,
                        Value = nextTokenState.TokenType == FileTokenType.NewLine
                         ? Environment.NewLine
                         : tokenBuilder.ToString()
                    };
                case TokenReadState.Multiple:
                    if (nextTokenState.ReadToBytes == null)
                    {
                        throw new InvalidOperationException("Encountered multiple multi-state tokens in the same process.");
                    }
                    tokenBuilder.Append(nextTokenState.Token.AsCharString());
                    return await ReadToBytesAsync(nextTokenState.TokenType, tokenBuilder,
                        nextTokenState.ReadToBytes, cancellationToken);
            }

            throw new InvalidOperationException($"There was an unexpected read state encountered, {nextTokenState.ReadState}, when handling multi-state tokens.");
        }

        private async ValueTask<FileToken> ReadToBytesAsync(FileTokenType tokenType, StringBuilder tokenBuilder, int[] expectedBytes, CancellationToken cancellationToken)
        {
            var currentMatchingIndex = 0;
            var iterationsUntilYield = _options.IterationsUntilYield;
            while (_fileStream!.Position != _fileStream.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var fileByte = _fileStream.ReadByte();
                tokenBuilder.Append(fileByte.AsCharString());

                if (expectedBytes[currentMatchingIndex] == fileByte)
                {
                    currentMatchingIndex++;
                    if (currentMatchingIndex >= expectedBytes.Length)
                    {
                        return new FileToken()
                        {
                            TokenType = tokenType,
                            Value = tokenBuilder.ToString(),
                        };
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
