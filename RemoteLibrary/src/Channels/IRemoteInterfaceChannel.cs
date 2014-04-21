using System.Threading.Tasks;
using RemoteLibrary.Messages;

namespace RemoteLibrary.Channels
{
    public interface IRemoteInterfaceChannel
    {
        bool IsConnected { get; }
        Task<RemoteInterfaceMessage> SendMessageAndWaitForResponse(RemoteInterfaceMessage message);
    }
}