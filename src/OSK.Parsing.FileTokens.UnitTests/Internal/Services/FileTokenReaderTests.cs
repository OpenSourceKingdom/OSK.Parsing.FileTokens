using Moq;
using OSK.Parsing.FileTokens.Internal.Services;
using OSK.Parsing.FileTokens.Models;
using OSK.Parsing.FileTokens.Options;
using OSK.Parsing.FileTokens.Ports;
using System.Text;
using Xunit;

namespace OSK.Parsing.FileTokens.UnitTests.Internal.Services
{
    [Collection("Sequential")]
    public class FileTokenReaderTests: IDisposable
    {
        #region Variables

        private readonly string _testDirectory;
        private readonly string _classContent;
        private readonly FileStream _fileStream;
        private readonly Mock<ITokenStateHandler> _tokenStateHandlerMock;
        private readonly FileTokenReader _fileTokenReader;

        #endregion

        #region Constructors

        public FileTokenReaderTests()
        {
            _testDirectory = Path.Combine(".", "TestData");
            _tokenStateHandlerMock = new Mock<ITokenStateHandler>();
            Directory.CreateDirectory(_testDirectory);
            var filePath = Path.Combine(_testDirectory, $"testCSharpFile.cs");
            Stream stream = File.Create(filePath);
            try
            {
                _classContent = "using System.Text;\r\nusing Xunit;\r\n\r\nnamespace OSK.Parsing.FileTokens.UnitTests.Helpers.Fixtures\r\n{\r\n    public class FileFixture : IDisposable\r\n    {\r\n        private static readonly string TestDirectoryTemplate = Path.Combine(\".\", \"TestData\");\r\n        private Encoding _encoding = Encoding.UTF8;\r\n\r\n        private string _testDirectory;\r\n\r\n        public FileFixture()\r\n        {\r\n            NewDirectory();\r\n        }\r\n\r\n        public void NewDirectory()\r\n        {\r\n            _testDirectory = TestDirectoryTemplate;\r\n            Directory.CreateDirectory(_testDirectory);\r\n\r\n            Assert.True(Directory.Exists(_testDirectory));\r\n        }\r\n    }";
                stream.Write(Encoding.UTF8.GetBytes(_classContent));
            }
            finally
            {
                stream.Dispose();
            }

            _fileStream = File.OpenRead(filePath);
            _fileTokenReader = new FileTokenReader(_fileStream, _tokenStateHandlerMock.Object, new FileTokenReaderOptions());
        }

        #endregion

        #region Disposable

        public void Dispose()
        {
            _fileTokenReader.Dispose();
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, true);
            }

            Assert.False(Directory.Exists(_testDirectory));
        }

        #endregion

        #region FilePath

        [Fact]
        public void FilePath_ReturnsExpectedFilePath()
        {
            Assert.Equal(_fileStream.Name, _fileTokenReader.FilePath);
        }

        #endregion

        #region ReadTokenAsync

        [Fact]
        public async Task ReadTokenAsync_CancellationTokenCancellationRequested_ThrowsOperationCancelledException()
        {
            // Arrange
            using var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.Cancel();

            // Act/Assert
            await Assert.ThrowsAsync<OperationCanceledException>(async () => await _fileTokenReader.ReadTokenAsync(cancellationTokenSource.Token));
        }

        [Fact]
        public async Task ReadTokenAsync_InitialToken_InitialTokenIsResetToken_ThrowsInvalidOperationException()
        {
            // Arrange
            _tokenStateHandlerMock.Setup(m => m.GetInitialTokenState(It.IsAny<int>()))
                .Returns(new TokenState(FileTokenType.Text, TokenReadState.Reset));

            // Act/Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _fileTokenReader.ReadTokenAsync());
            _fileTokenReader.Dispose();
        }

        [Fact]
        public async Task ReadTokenAsync_InitialToken_FileStreamAtEndPosition_ReturnsEndOfFileToken()
        {
            // Arrange
            _fileStream.Position = _fileStream.Length;

            // Act
            var result = await _fileTokenReader.ReadTokenAsync();

            // Assert
            Assert.Equal(FileTokenType.EndOfFile, result.TokenType);
            _fileTokenReader.Dispose();
        }

        [Fact]
        public async Task ReadTokenAsync_InitialToken_TokenStateIsSingleToken_ReturnsTokenState()
        {
            // Arrange
            var tokenState = new TokenState(FileTokenType.EndOfStatement, TokenReadState.SingleRead, 1, 2, 3);
            _tokenStateHandlerMock.Setup(m => m.GetInitialTokenState(It.IsAny<int>()))
                .Returns(tokenState);

            // Act
            var result = await _fileTokenReader.ReadTokenAsync();

            // Assert
            Assert.Equal(tokenState.TokenType, result.TokenType);
            Assert.True(tokenState.Tokens.SequenceEqual(result.RawTokens));
            Assert.Equal(1, _fileStream.Position);
            _fileTokenReader.Dispose();
        }

        [Fact]
        public async Task ReadTokenAsync_InitialToken_TokenStateIsEndReadToken_ReturnsTokenState()
        {
            // Arrange
            var tokenState = new TokenState(FileTokenType.Assignment, TokenReadState.EndRead, 1, 2, 3);
            _tokenStateHandlerMock.Setup(m => m.GetInitialTokenState(It.IsAny<int>()))
                .Returns(tokenState);

            // Act
            var result = await _fileTokenReader.ReadTokenAsync();

            // Assert
            Assert.Equal(tokenState.TokenType, result.TokenType);
            Assert.True(tokenState.Tokens.SequenceEqual(result.RawTokens));
            Assert.Equal(1, _fileStream.Position);
            _fileTokenReader.Dispose();
        }

        [Fact]
        public async Task ReadTokenAsync_NextToken_TokenStateIsEndReadToken_ReturnsNextTokenState()
        {
            // Arrange
            var initialTokenState = new TokenState(FileTokenType.Assignment, TokenReadState.ReadNext, 1, 2, 3);
            _tokenStateHandlerMock.Setup(m => m.GetInitialTokenState(It.IsAny<int>()))
                .Returns(initialTokenState);
            var nextTokenState = new TokenState(FileTokenType.Comment, TokenReadState.EndRead, 1, 2, 3);
            _tokenStateHandlerMock.Setup(m => m.GetNextTokenState(It.IsAny<TokenState>(), It.IsAny<int>()))
                .Returns(nextTokenState);

            // Act
            var result = await _fileTokenReader.ReadTokenAsync();

            // Assert
            Assert.Equal(nextTokenState.TokenType, result.TokenType);
            Assert.True(nextTokenState.Tokens.SequenceEqual(result.RawTokens));
            Assert.Equal(2, _fileStream.Position);
            _fileTokenReader.Dispose();
        }

        [Fact]
        public async Task ReadTokenAsync_NextToken_TokenStateIsIgnoredThenEndReadToken_ReturnsEndTokenState()
        {
            // Arrange
            var initialTokenState = new TokenState(FileTokenType.Assignment, TokenReadState.ReadNext, 1, 2, 3);
            _tokenStateHandlerMock.Setup(m => m.GetInitialTokenState(It.IsAny<int>()))
                .Returns(() =>
                {
                    return initialTokenState;
                });
            var i = 0;
            var nextTokenState1 = new TokenState(FileTokenType.Ignore, TokenReadState.EndRead, 1, 2, 3);
            var nextTokenState2 = new TokenState(FileTokenType.Separator, TokenReadState.EndRead, 5, 3, 3);
            _tokenStateHandlerMock.Setup(m => m.GetNextTokenState(It.IsAny<TokenState>(), It.IsAny<int>()))
                .Returns((TokenState _, int _) =>
                {
                    i++;
                    return i == 1 ? nextTokenState1
                     : nextTokenState2;
                });

            // Act
            var result = await _fileTokenReader.ReadTokenAsync();

            // Assert
            Assert.Equal(nextTokenState2.TokenType, result.TokenType);
            Assert.True(nextTokenState2.Tokens.SequenceEqual(result.RawTokens));
            Assert.Equal(3, _fileStream.Position);
            _fileTokenReader.Dispose();
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(5)]
        public async Task ReadTokenAsync_NextToken_ResetTokenResetsFileStreamBack_ReturnsExpectedToken(int resetCharacters)
        {
            // Arrange
            var tokens = Enumerable.Range(0, resetCharacters)
                .Select(_ => new TokenState(FileTokenType.Comment, TokenReadState.ReadNext, 1))
                .ToArray();

            _tokenStateHandlerMock.Setup(m => m.GetInitialTokenState(It.IsAny<int>()))
                .Returns(tokens[0]);

            var i = 1;
            var finalTokenState = new TokenState(FileTokenType.Delimeter, TokenReadState.EndRead, 54321);
            _tokenStateHandlerMock.Setup(m => m.GetNextTokenState(It.IsAny<TokenState>(), It.IsAny<int>()))
                .Returns((TokenState state, int _) =>
                {
                    var tokenState = i == tokens.Length
                     ? new TokenState(FileTokenType.Comment, TokenReadState.Reset, 1, 2, 3)
                     : i > tokens.Length
                       ? finalTokenState
                       : new TokenState(FileTokenType.Comment, TokenReadState.ReadNext, state.Tokens.Append(1).ToArray());
                    i++;
                    return tokenState;
                });

            // Act
            var result = await _fileTokenReader.ReadTokenAsync();

            // Assert
            Assert.Equal(finalTokenState.Tokens, result.RawTokens);
            Assert.Equal(finalTokenState.TokenType, result.TokenType);
            Assert.Equal(1, _fileStream.Position);
        }

        [Fact]
        public async Task ReadTokenAsync_NextToken_ReadTextUntilCompletion_ReturnsExpectedTextToken()
        {
            // Arrange
            var tokens = Enumerable.Range(0, 10)
                .Select(_ => new TokenState(FileTokenType.Text, TokenReadState.ReadNext, 1))
                .ToArray();

            _tokenStateHandlerMock.Setup(m => m.GetInitialTokenState(It.IsAny<int>()))
                .Returns(tokens[0]);

            var i = 1;
            _tokenStateHandlerMock.Setup(m => m.GetNextTokenState(It.IsAny<TokenState>(), It.IsAny<int>()))
                .Returns((TokenState state, int _) =>
                {
                    var tokenState = i == tokens.Length
                       ? new TokenState(FileTokenType.Text, TokenReadState.EndRead, state.Tokens)
                       : new TokenState(FileTokenType.Text, TokenReadState.ReadNext, state.Tokens.Append(1).ToArray());
                    i++;
                    return tokenState;
                });

            // Act
            var result = await _fileTokenReader.ReadTokenAsync();

            // Assert
            Assert.Equal(tokens.SelectMany(t => t.Tokens), result.RawTokens);
            Assert.Equal(FileTokenType.Text, result.TokenType);
            Assert.Equal(tokens.Length, _fileStream.Position);
        }

        #endregion

        #region ReadToEndTokenAsync

        [Fact]
        public async Task ReadToEndTokenAsync_NullFileToken_ThrowsArgumentNullException()
        {
            // Arrange/Act/Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () => await _fileTokenReader.ReadToEndTokenAsync(null!));
        }

        [Fact]
        public async Task ReadToEndTokenAsync_FileStreamIsAtEndOfFile_ReturnsEndOfFileToken()
        {
            // Arrange
            _fileStream.Position = _fileStream.Length;

            // Act
            var result = await _fileTokenReader.ReadToEndTokenAsync(new FileToken(FileTokenType.Text, 1));

            // Assert
            Assert.Equal(FileTokenType.EndOfFile, result.TokenType);
        }

        [Fact]
        public async Task ReadToEndTokenAsync_FileTokenHasNoEndTokenToRead_ReturnsFileToken()
        {
            // Arrange
            var fileToken = new FileToken(FileTokenType.Text, 1);

            _tokenStateHandlerMock.Setup(m => m.GetEndToken(It.IsAny<SingleReadToken>()))
                .Returns((SingleReadToken?)null);

            // Act
            var result = await _fileTokenReader.ReadToEndTokenAsync(fileToken);

            // Assert
            Assert.Equal(fileToken, result);
        }

        [Fact]
        public async Task ReadToEndTokenAsync_FileTokenEndTokenNotFound_ThrowsInvalidOperationException()
        {
            // Arrange
            var fileToken = new FileToken(FileTokenType.Text, 1);

            _tokenStateHandlerMock.Setup(m => m.GetEndToken(It.IsAny<SingleReadToken>()))
                .Returns(new SingleReadToken(FileTokenType.NewLine, '$'));

            // Act/Assert
            await Assert.ThrowsAsync<InvalidOperationException>(async () => await _fileTokenReader.ReadToEndTokenAsync(fileToken));
        }

        [Theory]
        [InlineData(10, 1)]
        [InlineData(20, 25)]
        public async Task ReadToEndTokenAsync_FileTokenEndTokenHasValue_ReadsToSpecifiedBytes(int classContentStartIndex, int classContentCharacterCount)
        {
            // Arrange
            var fileToken = new FileToken(FileTokenType.Text, 1);

            var readToBytes = _classContent.Skip(classContentStartIndex).Take(classContentCharacterCount);
            _tokenStateHandlerMock.Setup(m => m.GetEndToken(It.IsAny<SingleReadToken>()))
                .Returns(new SingleReadToken(FileTokenType.NewLine, readToBytes.Select(c => (int)c).ToArray()));

            // Act
            var result = await _fileTokenReader.ReadToEndTokenAsync(fileToken);

            // Assert
            var expectedReadCount = classContentStartIndex + classContentCharacterCount;
            Assert.Equal(expectedReadCount + fileToken.RawTokens.Length, result.RawTokens.Length);
            Assert.Equal(_fileStream.Position, expectedReadCount);
        }

        #endregion
    }
}
