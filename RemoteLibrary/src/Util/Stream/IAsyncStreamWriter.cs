using System.Threading;
using System.Threading.Tasks;

namespace RemoteLibrary.Util.Stream
{
    public interface IAsyncStreamWriter
    {
        Task WriteBytesAsync(byte[] bytes, CancellationToken cancelToken);
        Task WriteBytesAsync(byte[] bytes, int offset, int count, CancellationToken cancelToken);
    }
}