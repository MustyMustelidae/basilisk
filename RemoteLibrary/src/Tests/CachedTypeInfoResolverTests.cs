using NUnit.Framework;
using RemoteLibrary.Util;
using RemoteLibrary.Util.Cached;

namespace RemoteLibrary.Tests
{
    [TestFixture]
    public class CachedTypeInfoResolverTests : GenericCachedTypeInfoResolverTests<CachedTypeInfoResolver>
    {
        public override CachedTypeInfoResolver GetNewInfoResolver()
        {
            CachedTypeInfoResolver.ResetTypeInfo();
            return new CachedTypeInfoResolver();
        }
    }
}