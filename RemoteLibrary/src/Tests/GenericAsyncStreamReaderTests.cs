using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using Moq;
using NUnit.Framework;
using RemoteLibrary.Util.StreamIO;

namespace RemoteLibrary.Tests
{
    public abstract class GenericAsyncStreamReaderTests<T> where T : IAsyncStreamReader
    {
        private byte[] TestBytes
        {
            get
            {
                var guid = new Guid(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
                return guid.ToByteArray();
            }
        }

        protected abstract T GetStreamReader(Stream stream);

        private MemoryStream GetTestStream()
        {
            var stream = new MemoryStream(TestBytes);
            stream.SetLength(TestBytes.Length);
            return stream;
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public void ThrowsExceptionOnNullStream()
        {
            GetStreamReader(null);
        }

        [Test]
        [ExpectedException(typeof (ArgumentException))]
        public void ThrowsExceptionOnUnreadableStream()
        {
            var streamMock = new Mock<Stream>();
            streamMock.Setup(stream => stream.CanRead).Returns(false);
            GetStreamReader(streamMock.Object);
        }

        [TestCase(-1)]
        [ExpectedException(typeof (ArgumentOutOfRangeException))]
        public async void ThrowsExceptionInvalidCount(int count)
        {
            using (var cancelSource = new CancellationTokenSource())
            using (var stream = new MemoryStream())
            {
                var streamReader = GetStreamReader(stream);
                await streamReader.ReadBytesAsync(count, cancelSource.Token);
            }
        }

        [Test]
        [ExpectedException(typeof (ArgumentNullException))]
        public async void ThrowsExceptionOnNullCancellationToken()
        {
            using (var stream = new MemoryStream())
            {
                var streamReader = GetStreamReader(stream);
                await streamReader.ReadBytesAsync(0, default(CancellationToken));
            }
        }

        [Test]
        [Ignore("Fail behavior not yet defined")]
        public async void FailsToReadOnEmptyStream()
        {
            const int count = 10;
            using (var cancelSource = new CancellationTokenSource())
            using (var stream = new MemoryStream())
            {
                var streamReader = GetStreamReader(stream);
                var readBytes = await streamReader.ReadBytesAsync(count, cancelSource.Token);
                Assert.IsTrue(readBytes.Length == 0);
            }
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(4)]
        public async void CanRead(int count)
        {
            using (var cancelSource = new CancellationTokenSource())
            using (var stream = GetTestStream())
            {
                Debug.Assert(count < stream.Length, "Test value larger than available bytes");
                var streamReader = GetStreamReader(stream);
                var readBytes = await streamReader.ReadBytesAsync(count, cancelSource.Token);
                var expected = TestBytes.Take(count);
                CollectionAssert.AreEquivalent(expected, readBytes);
            }
        }

        [TestCase(0)]
        [TestCase(2)]
        [TestCase(4)]
        public async void CanReadFromNonSeekableStream(int count)
        {
            using (var cancelSource = new CancellationTokenSource())
            {
                var streamMock = new Mock<MemoryStream>(TestBytes) {CallBase = true};
                streamMock.Setup(memoryStream => memoryStream.CanSeek)
                    .Returns(false);

                var stream = streamMock.Object;

                Debug.Assert(count < stream.Length, "Test value larger than available bytes");

                var streamReader = GetStreamReader(streamMock.Object);
                var readBytes = await streamReader.ReadBytesAsync(count, cancelSource.Token);
                var expected = TestBytes.Take(count);

                CollectionAssert.AreEquivalent(expected, readBytes);
            }
        }
    }
}