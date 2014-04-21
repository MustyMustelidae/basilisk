using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteLibrary.Util.Stream
{
    public class AsyncStreamWriter : IAsyncStreamWriter
    {
        public readonly System.IO.Stream WriteStream;

        public AsyncStreamWriter(System.IO.Stream writeStream)
        {
            Contract.Requires(writeStream != null);
            Contract.Requires(writeStream.CanWrite);
            WriteStream = writeStream;
        }


        public async Task WriteBytesAsync(byte[] bytes, CancellationToken cancelToken)
        {
            if (bytes == null) throw new ArgumentNullException();
            await WriteBytesAsync(bytes, 0, bytes.Length, cancelToken);
        }

        public async Task WriteBytesAsync(byte[] bytes, int offset, int count, CancellationToken cancelToken)
        {
            if (cancelToken == default(CancellationToken)) throw new ArgumentNullException("cancelToken");
            if (cancelToken == null) throw new ArgumentNullException("cancelToken");
            if (bytes == null) throw new ArgumentNullException("bytes");

            if (count > bytes.Length) throw new ArgumentOutOfRangeException("count", "Count is larger than byte array");
            if (offset > bytes.Length)
                throw new ArgumentOutOfRangeException("offset", "Offset is larger than byte array");
            if (count + offset > bytes.Length)
                throw new ArgumentOutOfRangeException("count", "Count plus offset is larger than byte array");

            if (count < 0) throw new ArgumentOutOfRangeException("count", "Count must be greater than zero");
            if (offset < 0) throw new ArgumentOutOfRangeException("offset", "Offset must be greater than zero");

            await WriteStream.WriteAsync(bytes, offset, count, cancelToken);
            await WriteStream.FlushAsync(cancelToken);
        }
    }
}