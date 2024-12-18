# OSK.Parsing.FileTokens
This project provides a file parsing utility that is capable of reading a file as a flow of language, or syntax, tokens rather than a generic stream
of bytes and is helpful in situations where a consumer would like the ability to know what type of tokens are being read for a programming language for file
or other project based analysis.

The primary path to the library is the `FileTokenParser` which can be used to create an `IFileTokenReader` that is capable of reading a file stream that is
then interpreted by an implementation of an `ITokenStateHandler`. Essentially, the token state handler informs the file token reader the type of language token
is being read.  

In terms of the internal logic, a `SingleReadToken` is used when a token read by the token reader is a single read token, whereas a `MultiReadToken` symbolizes a 
token in a file that could cover multiple lines. For example, in C#, a closure token such as a `(` or `{` would be a single read token but a multi line comment would be
a multi token that has a starting token `/*` and a closing token `*/`. A `TokenState` is an object that provides necessary information for the file token reader to process
reading file tokens from a file. Token States will use one of the available values for `TokenReadState` to indicate if a token has been fully parsed from the file or if further
processing is necessary.

Information provided from a `FileToken` will describe if the token is a closure, delimeter, text, 

The library provides:
* `GenericTokenStateHandler` - A base class that provides most implementation details for generic cases where only the syntax tokens being used need to be changed. Subclasses will only need to define the actual token strings that are available for the language.
* A default implementation and example for a general C# token state handler can be seen in `DefaultTokenStateHandler`. Consumers wanting to use the
library for a more specific syntax of C# or another language will need to create an implementation of an `ITokenStateHandler` and pass it into the corresponding 
overload in the `FileTokenParser`.

# Contributions and Issues
Any and all contributions are appreciated! Please be sure to follow the branch naming convention OSK-{issue number}-{deliminated}-{branch}-{name} as current workflows rely on it for automatic issue closure. Please submit issues for discussion and tracking using the github issue tracker.