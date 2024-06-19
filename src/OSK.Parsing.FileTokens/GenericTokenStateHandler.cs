using OSK.Parsing.FileTokens.Models;
using OSK.Parsing.FileTokens.Ports;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OSK.Parsing.FileTokens
{
    public abstract class GenericTokenStateHandler : ITokenStateHandler
    {
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

        private readonly int _endOfStatement;
        private readonly int _assignmentOperator;
        private readonly HashSet<int> _delimeterTokens;
        private readonly HashSet<int> _separatorTokens;

        private readonly Dictionary<int, int> _closureStartTokens;
        private readonly HashSet<int> _closureEndTokens;

        #endregion

        #region Constructors

        protected GenericTokenStateHandler(
            int endOfStatement, int assignmentOperator,
            IEnumerable<int> delimeterTokens, IEnumerable<int> separatorTokens,
            IEnumerable<ClosureToken> closureTokens)
        {
            _endOfStatement = endOfStatement;
            _assignmentOperator = assignmentOperator;
            _delimeterTokens = delimeterTokens?.ToHashSet() ?? throw new ArgumentNullException(nameof(delimeterTokens));
            _separatorTokens = separatorTokens?.ToHashSet() ?? throw new ArgumentNullException(nameof(separatorTokens));

            if (closureTokens == null)
            {
                throw new ArgumentNullException(nameof(closureTokens));
            }

            _closureStartTokens = new Dictionary<int, int>();
            _closureEndTokens = new HashSet<int>();
            foreach (var closureToken in closureTokens)
            {
                _closureStartTokens.Add(closureToken.ClosureStartToken, closureToken.ClosureEndToken);
                _closureEndTokens.Add(closureToken.ClosureEndToken);
            }
        }

        #endregion

        #region ITokenStateHandler

        public int? GetTokenEndValue(int character)
        {
            int? endValue = null;
            if (_closureStartTokens.TryGetValue(character, out var closureToken))
            {
                return closureToken;
            }
            if (character == _assignmentOperator)
            {
                endValue = _endOfStatement;
            }

            return endValue;
        }

        public TokenState GetTokenState(int character)
        {
            var tokenState = character switch
            {
                EndOfFileValue => new TokenState()
                {
                    TokenType = FileTokenType.EndOfFile,
                    ReadState = TokenReadState.Single,
                    Token = character
                },
                Equivalence => new TokenState()
                {
                    TokenType = FileTokenType.Assignment,
                    ReadState = TokenReadState.Single,
                    Token = character
                },
                NewLine => new TokenState()
                {
                    TokenType = FileTokenType.NewLine,
                    ReadState = TokenReadState.Single,
                    Token = character
                },
                CarriageReturn => new TokenState()
                {
                    TokenType = FileTokenType.NewLine,
                    ReadState = TokenReadState.Multiple,
                    Token = character
                },
                Slash => new TokenState()
                {
                    TokenType = FileTokenType.Comment,
                    ReadState = TokenReadState.Multiple,
                    Token = character
                },
                _ => null
            };

            if (tokenState != null)
            {
                return tokenState;
            }
            if (character == _endOfStatement)
            {
                return new TokenState()
                {
                    TokenType = FileTokenType.EndOfStatement,
                    ReadState = TokenReadState.Single,
                    Token = character
                };
            }
            if (_closureStartTokens.TryGetValue(character, out var closureEndToken))
            {
                return new TokenState()
                {
                    TokenType = FileTokenType.ClosureStart,
                    ReadState = TokenReadState.Single,
                    Token = character
                };
            }
            if (_closureEndTokens.Contains(character))
            {
                return new TokenState()
                {
                    TokenType = FileTokenType.ClosureEnd,
                    ReadState = TokenReadState.Single,
                    Token = character
                };
            }
            if (_delimeterTokens.Contains(character))
            {
                return new TokenState()
                {
                    TokenType = FileTokenType.Delimeter,
                    ReadState = TokenReadState.Single,
                    Token = character
                };
            }
            if (_separatorTokens.Contains(character))
            {
                return new TokenState()
                {
                    TokenType = FileTokenType.Separator,
                    ReadState = TokenReadState.Single,
                    Token = character
                };
            }

            return IsValidCharacter(character)
                ? new TokenState()
                {
                    TokenType = FileTokenType.Text,
                    ReadState = TokenReadState.Multiple,
                    Token = character
                }
                : new TokenState()
                {
                    TokenType = FileTokenType.Ignore,
                    ReadState = TokenReadState.Single,
                    Token = character
                };
        }

        public virtual TokenState GetTokenState(TokenState previousState, int character)
        {
            switch (previousState.TokenType)
            {
                case FileTokenType.Comment:
                    if (character == Slash)
                    {
                        return new TokenState()
                        {
                            ReadState = TokenReadState.Multiple,
                            TokenType = FileTokenType.Comment,
                            ReadToBytes = new int[]
                            {
                                NewLine
                            },
                            Token = character
                        };
                    }
                    if (character == Asterisk)
                    {
                        return new TokenState()
                        {
                            ReadState = TokenReadState.Multiple,
                            TokenType = FileTokenType.Comment,
                            ReadToBytes = new int[]
                            {
                                Asterisk, Slash
                            },
                            Token = character
                        };
                    }
                    break;
                case FileTokenType.NewLine:
                    if (character == NewLine)
                    {
                        return new TokenState()
                        {
                            ReadState = TokenReadState.Single,
                            TokenType = FileTokenType.NewLine,
                            Token = character
                        };
                    }
                    break;
            }

            return new TokenState()
            {
                ReadState = TokenReadState.Reset,
                TokenType = FileTokenType.Ignore,
                Token = character
            };
        }

        #endregion

        #region Helpers

        // built-in .NET check for ASCII:
        // Per http://www.unicode.org/glossary/#ASCII, ASCII is only U+0000..U+007F.
        protected virtual bool IsValidCharacter(int character)
            => (uint)character <= '\x007f';

        #endregion
    }
}
