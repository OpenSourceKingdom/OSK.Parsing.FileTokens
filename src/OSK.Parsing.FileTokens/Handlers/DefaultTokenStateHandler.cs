using OSK.Parsing.FileTokens.Models;

namespace OSK.Parsing.FileTokens.Handlers
{
    /// <summary>
    /// A token state handler that should cover the general use case for C# files
    /// </summary>
    public sealed class DefaultTokenStateHandler : GenericTokenStateHandler
    {
        #region Static

        public static DefaultTokenStateHandler Instance = new DefaultTokenStateHandler();

        #endregion

        #region Variables

        public const int Tab = 9;
        public const int Space = ' ';
        public const int CarriageReturn = 13;
        public const int NewLine = 10;
        public const int EndOfFileValue = -1;
        public const int Colon = ':';
        public const int SemiColon = ';';
        public const int Comma = ',';
        public const int Slash = '/';
        public const int Asterisk = '*';
        public const int Equivalence = '=';
        public const int OpenParentheses = '(';
        public const int CloseParentheses = ')';
        public const int OpenBracket = '{';
        public const int CloseBracket = '}';

        private static SingleReadToken[] SingleTokens => new[]
        {
            new SingleReadToken(FileTokenType.EndOfStatement, SemiColon),
            new SingleReadToken(FileTokenType.Assignment, Equivalence),
            new SingleReadToken(FileTokenType.Delimeter, Space),
            new SingleReadToken(FileTokenType.Delimeter, Colon),
            new SingleReadToken(FileTokenType.Delimeter, Tab),
            new SingleReadToken(FileTokenType.Separator, Comma),
        };

        private static MultiReadToken[] MultiTokens => new[]
        {
            new MultiReadToken(new SingleReadToken(FileTokenType.ClosureStart, OpenParentheses),
                new SingleReadToken(FileTokenType.ClosureEnd, CloseParentheses)),
            new MultiReadToken(new SingleReadToken(FileTokenType.ClosureStart, OpenBracket),
                new SingleReadToken(FileTokenType.ClosureEnd, CloseBracket))
        };

        #endregion

        private DefaultTokenStateHandler()
            : base(SingleTokens, MultiTokens)
        {
        }
    }
}
