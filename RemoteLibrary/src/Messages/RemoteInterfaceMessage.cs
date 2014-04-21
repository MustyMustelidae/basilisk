using System;
using ProtoBuf;
using RemoteLibrary.Messages.Results;
using Shared;

namespace RemoteLibrary.Messages
{
    [ProtoContract]
    [ProtoInclude(100, typeof (RemoteInvocation))]
    [ProtoInclude(200, typeof (RemoteInvocationResult))]
    public abstract class RemoteInterfaceMessage
    {
        protected RemoteInterfaceMessage()
        {
        }

        protected RemoteInterfaceMessage(IGuidProvider guidProvider)
        {
            if (guidProvider == null) throw new ArgumentNullException("guidProvider");
            MessageGuid = guidProvider.GetNewGuid();
        }

        [ProtoMember(1)]
        public Guid MessageGuid { get; set; }
    }
}