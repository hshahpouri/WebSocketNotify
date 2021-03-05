using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketNotify
{
    public class WSNotifyData
    {
        public string Key { get; }
        public WebSocket Client { get; }
        public IPAddress IP { get; }
        public TaskCompletionSource<WebSocket> SocketTCS { get; }
        public CancellationTokenSource ThreadCancellation { get; }

        public WSNotifyData(string key, IPAddress ip, WebSocket webSocket, TaskCompletionSource<WebSocket> tcs)
        {
            Key = key;
            IP = ip;
            Client = webSocket;
            SocketTCS = tcs;
            ThreadCancellation = new CancellationTokenSource();
        }
    }
}
