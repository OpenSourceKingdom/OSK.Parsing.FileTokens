using OSK.Hexagonal.MetaData;
using OSK.Parsing.FileTokens.Models;

namespace OSK.Parsing.FileTokens.Ports
{
    /// <summary>
    /// A token state handler provides the types of tokens being read from a file stream based on the bytes being read.
    /// </summary>
    [HexagonalIntegration(HexagonalIntegrationType.IntegrationOptional)]
    public interface ITokenStateHandler
    {
        /// <summary>
        /// Provides the corresponding end token to a <see cref="SingleReadToken"/> that is passed. This is requesting the token state handler to 
        /// determine if the specified single read token is potentially related to a <see cref="MultiReadToken"/>
        /// </summary>
        /// <param name="singleToken">The language token read from the tile</param>
        /// <returns>A corresponding end token, if one is possible</returns>
        SingleReadToken? GetEndToken(SingleReadToken singleToken);

        /// <summary>
        /// Reads the initial token state. This may result in a <see cref="FileToken"/> that has been completed, indicated by <see cref="TokenReadState.SingleRead"/>, in the case of a single read token
        /// that only has a single syntax token to read.
        /// </summary>
        /// <param name="character">The byte character read from the file stream</param>
        /// <returns></returns>
        TokenState GetInitialTokenState(int character);

        /// <summary>
        /// This continues the process of reading a token and allows for further token analysis by providing the current state to used in processing.
        /// Implementations of <see cref="ITokenStateHandler"/> should expect this method to be called after <see cref="GetInitialTokenState(int)"/> if
        /// token processing has not been completed by returning a state with <see cref="TokenReadState.EndRead"/>
        /// </summary>
        /// <param name="previousState">The previous state incompleted token parsing state returned either by <see cref="GetInitialTokenState(int)"/> or 
        /// <see cref="GetNextTokenState(TokenState, int)"/></param>
        /// <param name="character">The byte character that was read from the file stream</param>
        /// <returns>The new <see cref="TokenState"/></returns>
        TokenState GetNextTokenState(TokenState previousState, int character);
    }
}
