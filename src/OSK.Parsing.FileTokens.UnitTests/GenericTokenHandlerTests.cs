using OSK.Parsing.FileTokens.Models;
using OSK.Parsing.FileTokens.UnitTests.Helpers;
using System.ComponentModel.DataAnnotations;
using System.Xml;
using Xunit;

namespace OSK.Parsing.FileTokens.UnitTests
{
    public class GenericTokenHandlerTests
    {
        #region Variables

        private readonly SingleReadToken[] _singleTokens;
        private readonly MultiReadToken[] _multiTokens;
        private readonly TestTokenHandler _tokenHandler;

        #endregion

        #region Constructors

        public GenericTokenHandlerTests()
        {
            _singleTokens =
            [
                new SingleReadToken(FileTokenType.Separator, 1),
                new SingleReadToken(FileTokenType.ClosureStart, 2, 3),
                new SingleReadToken(FileTokenType.Assignment, 4, 5, 6),
                new SingleReadToken(FileTokenType.EndOfStatement, 4, 7, 6)
            ];
            _multiTokens =
            [
                new MultiReadToken(new SingleReadToken(FileTokenType.ClosureStart, 7), new SingleReadToken(FileTokenType.ClosureEnd, 8)),
                new MultiReadToken(new SingleReadToken(FileTokenType.Delimeter, 9, 10), new SingleReadToken(FileTokenType.ClosureEnd, 11)),
                new MultiReadToken(new SingleReadToken(FileTokenType.Comment, 12, 13, 14), new SingleReadToken(FileTokenType.Comment, 15))
            ];
            _tokenHandler = new TestTokenHandler(_singleTokens, _multiTokens);
        }

        #endregion

        #region GetEndToken

        [Fact]
        public void GetEndToken_NullToken_ReturnsNull()
        {
            // Arrange/Act
            var result = _tokenHandler.GetEndToken(null!);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetEndToken_NoMultiTokenMatch_ReturnsNull()
        {
            // Arrange/Act
            var result = _tokenHandler.GetEndToken(new SingleReadToken(FileTokenType.NewLine, 5));

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public void GetEndToken_MultiTokenMatch_ReturnsEndToken()
        {
            // Arrange
            var multiToken = _multiTokens.First();

            // Act
            var result = _tokenHandler.GetEndToken(new SingleReadToken(multiToken.StartToken.TokenType, multiToken.StartToken.Tokens));

            // Assert
            Assert.Equal(multiToken.EndToken, result);
        }

        #endregion

        #region GetInitialTokenState(int)

        [Fact]
        public void GetInitialTokenState_SingleTokenMatch_TokenConfigurationForOneToken_ReturnsTokenStateWithSingleRead()
        {
            // Arrange
            var expectedToken = _singleTokens.First(t => t.Tokens.Length == 1);

            // Act
            var result = _tokenHandler.GetInitialTokenState(expectedToken.Tokens[0]);

            // Assert
            Assert.Equal(expectedToken.TokenType, result.TokenType);
            Assert.Equal(TokenReadState.SingleRead, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Single(result.Tokens);
            Assert.Equal(expectedToken.Tokens[0], result.Tokens[0]);
        }

        [Fact]
        public void GetInitialTokenState_SingleTokenMatch_TokenConfigurationForMultipleTokens_ReturnsTokenStateWithReadNext()
        {
            // Arrange
            var expectedToken = _singleTokens.First(t => t.Tokens.Length > 1);

            // Act
            var result = _tokenHandler.GetInitialTokenState(expectedToken.Tokens[0]);

            // Assert
            Assert.Equal(expectedToken.TokenType, result.TokenType);
            Assert.Equal(TokenReadState.ReadNext, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Single(result.Tokens);
            Assert.Equal(expectedToken.Tokens[0], result.Tokens[0]);
        }

        [Fact]
        public void GetInitialTokenState_MultiTokenMatch_TokenConfigurationForSingleStartToken_ReturnsExpectedMultiTokenState()
        {
            // Arrange
            var expectedMultiToken = _multiTokens.First(t => t.StartToken.Tokens.Length == 1);
            var expectedStartToken = expectedMultiToken.StartToken;

            // Act
            var result = _tokenHandler.GetInitialTokenState(expectedStartToken.Tokens[0]);

            // Assert
            Assert.Equal(expectedStartToken.TokenType, result.TokenType);
            Assert.Equal(TokenReadState.SingleRead, result.ReadState);
            Assert.NotNull(result.EndToken);
            Assert.Equal(expectedMultiToken.EndToken, result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Single(result.Tokens);
            Assert.Equal(expectedStartToken.Tokens[0], result.Tokens[0]);
        }

        [Fact]
        public void GetInitialTokenState_MultiTokenMatch_TokenConfigurationForMultiStartToken_ReturnsExpectedMultiTokenState()
        {
            // Arrange
            var expectedMultiToken = _multiTokens.First(t => t.StartToken.Tokens.Length > 1);
            var expectedStartToken = expectedMultiToken.StartToken;

            // Act
            var result = _tokenHandler.GetInitialTokenState(expectedStartToken.Tokens[0]);

            // Assert
            Assert.Equal(expectedStartToken.TokenType, result.TokenType);
            Assert.Equal(TokenReadState.ReadNext, result.ReadState);
            Assert.NotNull(result.EndToken);
            Assert.Equal(expectedMultiToken.EndToken, result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Single(result.Tokens);
            Assert.Equal(expectedStartToken.Tokens[0], result.Tokens[0]);
        }

        [Fact]
        public void GetInitialTokenState_NoTokenMatch_ValidTextCharacter_ReturnsExpectedTextTokenState()
        {
            // Arrange
            var tokenValue = 100;

            // Act
            var result = _tokenHandler.GetInitialTokenState(tokenValue);

            // Assert
            Assert.Equal(FileTokenType.Text, result.TokenType);
            Assert.Equal(TokenReadState.ReadNext, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Single(result.Tokens);
            Assert.Equal(tokenValue, result.Tokens[0]);
        }

        [Fact]
        public void GetInitialTokenState_NoTokenMatch_InvalidTextCharacter_ReturnsExpectedIgnoreTokenState()
        {
            // Arrange
            var tokenValue = 999;

            // Act
            var result = _tokenHandler.GetInitialTokenState(tokenValue);

            // Assert
            Assert.Equal(FileTokenType.Ignore, result.TokenType);
            Assert.Equal(TokenReadState.SingleRead, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Single(result.Tokens);
            Assert.Equal(tokenValue, result.Tokens[0]);
        }

        #endregion

        #region GetInitialTokenState(TokenState, int)

        [Fact]
        public void GetNextTokenState_NullTokenState_FallsBackToInitialMethod()
        {
            // Arrange
            var expectedToken = _singleTokens.First(t => t.Tokens.Length == 1);

            // Act
            var result = _tokenHandler.GetNextTokenState(null!, expectedToken.Tokens[0]);

            // Assert
            Assert.Equal(expectedToken.TokenType, result.TokenType);
            Assert.Equal(TokenReadState.SingleRead, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Single(result.Tokens);
            Assert.Equal(expectedToken.Tokens[0], result.Tokens[0]);
        }

        [Fact]
        public void GetNextTokenState_TextTokenState_FallsBackToInitialMethod_ValidText()
        {
            // Arrange
            var tokenValue = 100;

            // Act
            var result = _tokenHandler.GetNextTokenState(new TokenState(FileTokenType.Text, TokenReadState.ReadNext, tokenValue), tokenValue);

            // Assert
            Assert.Equal(FileTokenType.Text, result.TokenType);
            Assert.Equal(TokenReadState.ReadNext, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Equal(2, result.Tokens.Length);
            Assert.Equal((int[])[tokenValue, tokenValue], result.Tokens);
        }

        [Fact]
        public void GetNextTokenState_TextTokenState_FallsBackToInitialMethod_ReturnsTextTokenWithEndState()
        {
            // Arrange
            var tokenValue = 999;

            // Act
            var result = _tokenHandler.GetNextTokenState(new TokenState(FileTokenType.Text, TokenReadState.ReadNext, tokenValue), tokenValue);

            // Assert
            Assert.Equal(FileTokenType.Text, result.TokenType);
            Assert.Equal(TokenReadState.EndRead, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Single(result.Tokens);
            Assert.Equal(tokenValue, result.Tokens[0]);
        }

        [Theory]
        [InlineData(TokenReadState.SingleRead)]
        [InlineData(TokenReadState.EndRead)]
        public void GetNextTokenState_TokenReadStateIsNotReadNext_ReturnsOriginalTokenState(TokenReadState readState)
        {
            // Arrange
            var tokenState = new TokenState(FileTokenType.Assignment, readState, 1);

            // Act
            var result = _tokenHandler.GetNextTokenState(tokenState, 1);

            // Assert
            Assert.Equal(tokenState, result);
        }

        [Fact]
        public void GetNextTokenState_SingleTokenMatch_TokenConfigurationForTwoTokens_ReturnsTokenStateWithEndRead()
        {
            // Arrange
            var expectedToken = _singleTokens.First(t => t.Tokens.Length == 2);

            // Act
            var previousState = _tokenHandler.GetInitialTokenState(expectedToken.Tokens[0]);
            var result = _tokenHandler.GetNextTokenState(previousState, expectedToken.Tokens[1]);

            // Assert
            Assert.Equal(expectedToken.TokenType, result.TokenType);
            Assert.Equal(TokenReadState.EndRead, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Equal(result.Tokens, result.Tokens);
        }

        [Fact]
        public void GetNextTokenState_SingleTokenMatch_TokenConfigurationForMultipleTokens_ReturnsTokenStateWithReadNext()
        {
            // Arrange
            var expectedToken = _singleTokens.First(t => t.Tokens.Length > 2);

            // Act
            var previousState = _tokenHandler.GetInitialTokenState(expectedToken.Tokens[0]);
            var result = _tokenHandler.GetNextTokenState(previousState, expectedToken.Tokens[1]);

            // Assert
            Assert.Equal(expectedToken.TokenType, result.TokenType);
            Assert.Equal(TokenReadState.ReadNext, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Equal(expectedToken.Tokens.Take(2), result.Tokens);
        }

        [Fact]
        public void GetInitialTokenState_MultiTokenMatch_TokenConfigurationForTwoTokens_ReturnsExpectedMultiTokenState()
        {
            // Arrange
            var expectedMultiToken = _multiTokens.First(t => t.StartToken.Tokens.Length == 2);
            var expectedStartToken = expectedMultiToken.StartToken;

            // Act
            var previousToken = _tokenHandler.GetInitialTokenState(expectedStartToken.Tokens[0]);
            var result = _tokenHandler.GetNextTokenState(previousToken, expectedStartToken.Tokens[1]);

            // Assert
            Assert.Equal(expectedStartToken.TokenType, result.TokenType);
            Assert.Equal(TokenReadState.EndRead, result.ReadState);
            Assert.NotNull(result.EndToken);
            Assert.Equal(expectedMultiToken.EndToken, result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Equal(expectedStartToken.Tokens, result.Tokens);
        }

        [Fact]
        public void GetInitialTokenState_MultiTokenMatch_TokenConfigurationForMultipleTokens_ReturnsExpectedMultiTokenState()
        {
            // Arrange
            var expectedMultiToken = _multiTokens.First(t => t.StartToken.Tokens.Length > 2);
            var expectedStartToken = expectedMultiToken.StartToken;

            // Act
            var previousToken = _tokenHandler.GetInitialTokenState(expectedStartToken.Tokens[0]);
            var result = _tokenHandler.GetNextTokenState(previousToken, expectedStartToken.Tokens[1]);

            // Assert
            Assert.Equal(expectedStartToken.TokenType, result.TokenType);
            Assert.Equal(TokenReadState.ReadNext, result.ReadState);
            Assert.NotNull(result.EndToken);
            Assert.Equal(expectedMultiToken.EndToken, result.EndToken);
            Assert.Equal(0, result.TokenIndex);
            Assert.Equal(expectedStartToken.Tokens.Take(2), result.Tokens);
        }

        [Fact]
        public void GetInitialTokenState_MultiTokenMatchNotFound_ReturnsIgnoreTokenState()
        {
            // Arrange
            var expectedMultiToken = _multiTokens.First(t => t.StartToken.Tokens.Length == 2);

            // Act
            var previousToken = _tokenHandler.GetInitialTokenState(expectedMultiToken.StartToken.Tokens[0]);
            var result = _tokenHandler.GetNextTokenState(previousToken, 999);

            // Assert
            Assert.Equal(FileTokenType.Ignore, result.TokenType);
            Assert.Equal(TokenReadState.Reset, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(0, result.TokenIndex);
        }

        [Fact]
        public void GetInitialTokenState_PreviousTokenStateWasReset_IncrementsInternalSelectionIndexAndRestarts()
        {
            // Arrange
            var expectedInitialToken = _singleTokens.First(t => t.Tokens.Length == 3);
            var expectedFinalToken = _singleTokens.Last(t => t.Tokens.Length == 3);

            // Act
            var previousToken = _tokenHandler.GetInitialTokenState(expectedInitialToken.Tokens[0]);
            previousToken = _tokenHandler.GetNextTokenState(previousToken, expectedFinalToken.Tokens[1]);
            
            Assert.Equal(TokenReadState.Reset, previousToken.ReadState);

            previousToken = _tokenHandler.GetNextTokenState(previousToken, expectedInitialToken.Tokens[0]);
            var result = _tokenHandler.GetNextTokenState(previousToken, expectedFinalToken.Tokens[1]);

            // Assert
            Assert.Equal(expectedFinalToken.TokenType, result.TokenType);
            Assert.Equal(TokenReadState.ReadNext, result.ReadState);
            Assert.Null(result.EndToken);
            Assert.Equal(1, result.TokenIndex);
        }

        #endregion
    }
}
