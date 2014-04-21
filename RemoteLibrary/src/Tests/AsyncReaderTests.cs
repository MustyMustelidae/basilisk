using System.IO;
using NUnit.Framework;
using RemoteLibrary.Util;
using RemoteLibrary.Util.Stream;

namespace RemoteLibrary.Tests
{
    [TestFixture]
    public class AsyncReaderTests : GenericAsyncStreamReaderTests<AsyncStreamReader>
    {
        protected override AsyncStreamReader GetStreamReader(Stream stream)
        {
            return new AsyncStreamReader(stream);
        }
    }
}