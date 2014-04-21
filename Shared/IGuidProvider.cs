using System;

namespace Shared
{
    public interface IGuidProvider
    {
        Guid GetNewGuid();
    }
}