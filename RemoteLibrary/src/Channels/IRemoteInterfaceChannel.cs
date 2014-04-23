using System.Threading.Tasks;
using RemoteLibrary.Messages;

namespace RemoteLibrary.Channels
{
    public interface IRemoteInterfaceChannel
    {
        bool IsConnected { get; }
        Task<RemoteCallMessage> SendMessageAndWaitForResponse(RemoteCallMessage message);
    }
}