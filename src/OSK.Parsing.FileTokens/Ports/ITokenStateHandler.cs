using OSK.Parsing.FileTokens.Models;

namespace OSK.Parsing.FileTokens.Ports
{
    public interface ITokenStateHandler
    {
        SingleReadToken? GetEndToken(SingleReadToken singleToken);

        TokenState GetInitialTokenState(int character);

        TokenState GetNextTokenState(TokenState previousState, int character);
    }
}
