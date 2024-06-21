namespace OSK.Parsing.FileTokens.Models
{
    public enum TokenReadState
    {
        /// <summary>
        /// Signifies that the state machine reading tokens should reset to the previous position before the current token was read
        /// in order to correctly parse the token as the path did not lead to a completed token.
        /// </summary>
        Reset,
        /// <summary>
        /// The token being read is a single read token. i.e. only one byte was necessary to parse it
        /// </summary>
        SingleRead,
        /// <summary>
        /// Informs the state machine that the current token being parsed needs more data to determine the final result
        /// </summary>
        ReadNext,
        /// <summary>
        /// A state that indicates that a token using the <see cref="TokenReadState.ReadNext"/> state has finished being parsed
        /// </summary>
        EndRead
    }
}
