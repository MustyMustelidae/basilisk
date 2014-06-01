using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using RemoteLibrary.Util.StreamIO;

namespace RemoteLibrary.Tests
{
    public abstract class GenericAsyncWriterTests<T> where T : IAsyncStreamWriter
    {
        private static byte[] TestBytes
        {
            get
            {
                var testData = new Guid(0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 0);
                return testData.ToByteArray();
            }
        }

        protected abstract T GetNewWriter(Stream stream);

        [TestCase(0, 100)]
        [TestCase(100, 100)]
        [TestCase(5, 20)]
        [TestCase(100, 0)]
        [TestCase(-100, 0)]
        [TestCase(-10, -20)]
        [TestCase(-10, 20)]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public async void ThrowsExceptionOnInvalidCountOrOffset(int offset, int count)
        {
            Debug.Assert(
                count < 0 ||
                offset < 0 ||
                count > TestBytes.Length ||
                offset > TestBytes.Length ||
                count + offset > TestBytes.Length, "Test parameters are in non-exceptional range");

            using (var cancelSource = new CancellationTokenSource())
            using (var stream = new MemoryStream())
            {
                var asyncWriter = GetNewWriter(stream);

                await asyncWriter.WriteBytesAsync(TestBytes, offset, count, cancelSource.Token);
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async void ThrowsExceptionOnNullToken()
        {
            using (var stream = new MemoryStream())
            {
                var asyncWriter = GetNewWriter(stream);
                await asyncWriter.WriteBytesAsync(TestBytes, default(CancellationToken));
            }
        }

        [Test]
        [TestCase(0, 0)]
        [TestCase(0, 5)]
        [TestCase(5, 5)]
        [TestCase(5, 0)]
        public async void WritesToStreamWithOffset(int offset, int count)
        {
            Debug.Assert(count < TestBytes.Length, "Count exceeds test data length");

            Debug.Assert(offset < TestBytes.Length, "Offset exceeds test data length");

            Debug.Assert(count + offset < TestBytes.Length, "Count plus Offset exceeds test data length");

            using (var cancelSource = new CancellationTokenSource())
            using (var stream = new MemoryStream())
            {
                var asyncWriter = GetNewWriter(stream);

                await asyncWriter.WriteBytesAsync(TestBytes, offset, count, cancelSource.Token);
                var expectedBytes = TestBytes
                    .Skip(offset)
                    .Take(count);
                CollectionAssert.AreEqual(expectedBytes, stream.ToArray());
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async void ThrowsExceptionOnNullByteArray()
        {
            using (var cancelSource = new CancellationTokenSource())
            using (var stream = new MemoryStream())
            {
                var asyncWriter = GetNewWriter(stream);

                await asyncWriter.WriteBytesAsync(null, cancelSource.Token);
            }
        }

        [Test]
        public async void WritesToStream()
        {
            using (var cancelSource = new CancellationTokenSource())
            using (var stream = new MemoryStream())
            {
                var asyncWriter = GetNewWriter(stream);

                await asyncWriter.WriteBytesAsync(TestBytes, cancelSource.Token);
                CollectionAssert.AreEqual(TestBytes, stream.ToArray());
            }
        }
    }
}