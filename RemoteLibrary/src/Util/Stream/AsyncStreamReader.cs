using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteLibrary.Util.Stream
{
    public class AsyncStreamReader : IAsyncStreamReader
    {
        public readonly System.IO.Stream ReadStream;

        public AsyncStreamReader(System.IO.Stream readStream)
        {
            if (readStream == null) throw new ArgumentNullException("readStream");
            if (!readStream.CanRead) throw new ArgumentException("Stream must support read operations", "readStream");

            ReadStream = readStream;
        }

        public async Task<byte[]> ReadBytesAsync(int count, CancellationToken cancelToken)
        {
            if (count < 0) throw new ArgumentOutOfRangeException("count");
            var bytes = new byte[count];

            await WaitForBytes(count, cancelToken);
            var bytesRead = await ReadStream.ReadAsync(bytes, 0, count, cancelToken);

            if (bytesRead < count)
            {
                throw new EndOfStreamException();
            }

            return bytes;
        }

        private long BytesAvailable()
        {
            if (ReadStream.CanSeek)
            {
                return ReadStream.Length - ReadStream.Position;
            }
            return long.MaxValue;
        }

        private async Task WaitForBytes(long bytes, CancellationToken token)
        {
            if (token == default(CancellationToken)) throw new ArgumentNullException("token");
            const int waitPeriod = 100;
            while ((BytesAvailable() <= bytes))
            {
                if (!ReadStream.CanRead) return;
                if (token.IsCancellationRequested) return;
                await Task.Delay(waitPeriod, token);
            }
        }
    }
}