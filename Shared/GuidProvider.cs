using System;

namespace Shared
{
    public class GuidProvider : IGuidProvider
    {
        public Guid GetNewGuid()
        {
            return Guid.NewGuid();
        }
    }
}