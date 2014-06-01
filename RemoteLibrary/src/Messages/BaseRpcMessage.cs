using System;
using ProtoBuf;
using RemoteLibrary.Messages.Results;
using Shared;

namespace RemoteLibrary.Messages
{
    [ProtoContract]
    [ProtoInclude(100, typeof (RpcMessage))]
    [ProtoInclude(200, typeof (RemoteProxyInvocationResult))]
    public abstract class BaseRpcMessage
    {
        protected BaseRpcMessage()
        {
        }

        protected BaseRpcMessage(IGuidProvider guidProvider)
        {
            if (guidProvider == null) throw new ArgumentNullException("guidProvider");
            MessageGuid = guidProvider.GetNewGuid();
        }

        [ProtoMember(1)]
        public Guid MessageGuid { get; set; }
    }
}