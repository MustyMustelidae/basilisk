using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteLibrary.Util.StreamIO
{
    public class AsyncStreamWriter : IAsyncStreamWriter
    {
        public readonly Stream WriteStream;

        public AsyncStreamWriter(Stream writeStream)
        {
            if (writeStream == null) throw new ArgumentNullException();
            if (!writeStream.CanWrite) throw new ArgumentException("Stream must support write operations");
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