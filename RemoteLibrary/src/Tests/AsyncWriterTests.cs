using System.IO;
using NUnit.Framework;
using RemoteLibrary.Util;
using RemoteLibrary.Util.Stream;

namespace RemoteLibrary.Tests
{
    [TestFixture]
    public class AsyncWriterTests : GenericAsyncWriterTests<AsyncStreamWriter>
    {
        protected override AsyncStreamWriter GetNewWriter(Stream stream)
        {
            return new AsyncStreamWriter(stream);
        }
    }
}