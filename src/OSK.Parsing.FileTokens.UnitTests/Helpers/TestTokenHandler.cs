using OSK.Parsing.FileTokens.Models;

namespace OSK.Parsing.FileTokens.UnitTests.Helpers
{
    public class TestTokenHandler : GenericTokenStateHandler
    {
        public TestTokenHandler(SingleReadToken[] singleTokens, MultiReadToken[] multiTokens) 
            : base(singleTokens, multiTokens)
        {
        }
    }
}
