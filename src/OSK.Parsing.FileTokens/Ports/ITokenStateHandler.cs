using OSK.Parsing.FileTokens.Models;

namespace OSK.Parsing.FileTokens.Ports
{
    public interface ITokenStateHandler
    {
        int? GetTokenEndValue(int character);

        TokenState GetTokenState(int character);

        TokenState GetTokenState(TokenState previousState, int character);
    }
}
