using OSK.Hexagonal.MetaData;
using OSK.Parsing.FileTokens.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Parsing.FileTokens.Ports
{
    [HexagonalPort(HexagonalPort.Primary)]
    public interface IFileTokenReader : IDisposable
    {
        /// <summary>
        /// The location of the file the reader is pulling data from
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// Reads to an end token that is associated with the provided <see cref="FileToken"/>
        /// </summary>
        /// <param name="fileToken">The file token that will be used to lookup an associated end token,</param>
        /// <param name="cancellationToken">A token that can cancel the operaiton</param>
        /// <returns>The associated ending file token</returns>
        ValueTask<FileToken> ReadToEndTokenAsync(FileToken fileToken, CancellationToken cancellationToken = default);

        ValueTask<FileToken> ReadTokenAsync(CancellationToken cancellationToken = default);
    }
}
