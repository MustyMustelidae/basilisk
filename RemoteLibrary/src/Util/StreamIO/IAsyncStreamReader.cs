using System.Threading;
using System.Threading.Tasks;

namespace RemoteLibrary.Util.StreamIO
{
    public interface IAsyncStreamReader
    {
        Task<byte[]> ReadBytesAsync(int count, CancellationToken cancelToken);
    }
}