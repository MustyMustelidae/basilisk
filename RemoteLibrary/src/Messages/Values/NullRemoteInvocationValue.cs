using System;
using ProtoBuf;

namespace RemoteLibrary.Messages.Values
{
    [ProtoContract]
    public class NullRpcValue : RpcValue
    {
        public NullRpcValue(Type objectType) : base(objectType)
        {
        }
    }
}