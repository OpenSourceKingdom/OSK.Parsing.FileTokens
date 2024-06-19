using OSK.Parsing.FileTokens.Models;

namespace OSK.Parsing.FileTokens
{
    public sealed class DefaultTokenStateHandler : GenericTokenStateHandler
    {
        #region Static

        public static DefaultTokenStateHandler Instance = new DefaultTokenStateHandler();

        #endregion

        private DefaultTokenStateHandler()
            : base(endOfStatement: SemiColon, assignmentOperator: Equivalence,
                  delimeterTokens: new int[] { Space, Colon, Tab },
                  separatorTokens: new int[] { Comma },
                  closureTokens: new ClosureToken[] {
                      new ClosureToken() { ClosureStartToken = OpenParentheses, ClosureEndToken = CloseParentheses },
                      new ClosureToken() { ClosureStartToken = OpenBracket, ClosureEndToken = CloseBracket }
                })
        {
        }
    }
}
