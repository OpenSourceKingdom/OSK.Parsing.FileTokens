using OSK.Parsing.FileTokens.Models;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OSK.Parsing.FileTokens.Ports
{
    public interface IFileTokenReader : IDisposable
    {
        string FilePath { get; }

        ValueTask<FileToken> ReadToFileTokenEndValueAsync(FileToken fileToken, CancellationToken cancellationToken = default);

        ValueTask<FileToken> ReadTokenAsync(CancellationToken cancellationToken = default);
    }
}
