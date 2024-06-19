namespace OSK.Parsing.FileTokens.Models
{
    public enum FileTokenType
    {
        /// <summary>
        /// The begining of a character defined scope
        /// </summary>
        ClosureStart,
        /// <summary>
        /// The ending of a character defined scope
        /// </summary>
        ClosureEnd,
        /// <summary>
        /// A byte that signifies a block of valid textual input has ended
        /// </summary>
        Delimeter,
        /// <summary>
        /// A separator for a list of objects within a closure. i.e. a parameter list similar to a,b,c,..
        /// </summary>
        Separator,
        /// <summary>
        /// Valid textual input that must be parsed further by an interpreter of some sort
        /// </summary>
        Text,
        /// <summary>
        /// Signifies the end of file
        /// </summary>
        EndOfFile,
        /// <summary>
        /// A new line has been encountered
        /// </summary>
        NewLine,
        /// <summary>
        /// Signifies text that can be ignored as it has been marked as a form of description
        /// </summary>
        Comment,
        /// <summary>
        /// An operator that signifies the next set of input is for an assignment to a variable
        /// </summary>
        Assignment,
        /// <summary>
        /// Signifies the final end of a syntax statement
        /// </summary>
        EndOfStatement,
        /// <summary>
        /// Specifies the token should be ignored by the token reader or parser
        /// </summary>
        Ignore
    }
}
