using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WebSocketNotify
{
    public class WSNotifyHandler
    {
        /// <summary>
        /// Arguments:
        /// <list type="number">
        /// <item>Key</item>
        /// <item>Value</item>
        /// <item>Type (string/base64)</item>
        /// </list>
        /// </summary> 
        public event Action<string, string, WSNotifyMessageType> OnReceive;

        private readonly Dictionary<string, WSNotifyData> _wsClients;
        private readonly IOptionsMonitor<WSNotifyOptions> _options;

        public WSNotifyHandler(IOptionsMonitor<WSNotifyOptions> options)
        {
            _wsClients = new Dictionary<string, WSNotifyData>();
            _options = options;
        }

        /// <summary>
        /// List of client keys and their connection status
        /// </summary>
        public Dictionary<string, IPAddress> List() =>
            _wsClients.ToDictionary(t => t.Value.Key, q => q.Value.IP.MapToIPv4());

        /// <param name="key">Client key</param>
        /// <returns>Connection status of client</returns>
        public WebSocketState this[string key] =>
            _wsClients[key].Client.State;

        protected internal bool IsConnectionAllowed(IPAddress ip) =>
            _wsClients.Count(t => t.Value.IP.Equals(ip)) < _options.CurrentValue.ConnectionPerIP;

        public void Add(string key, IPAddress ip, WebSocket webSocket, TaskCompletionSource<WebSocket> socketTCS)
        {
            if (string.IsNullOrWhiteSpace(key) || webSocket is null)
            {
                socketTCS.TrySetCanceled();
                return;
            }

            foreach (var item in _wsClients.Where(c => c.Value.Client.State == WebSocketState.Closed))
            {
                _wsClients.Remove(item.Key, out WSNotifyData removedItem);
                removedItem.SocketTCS.TrySetResult(removedItem.Client);
            }

            key = key.Replace("+", "-").Replace("/", "_").Replace("=", "");

            var data = new WSNotifyData(key, ip, webSocket, socketTCS);
            _wsClients[key] = data;

            BindReceiver(data);
        }

        public async void RemoveAsync(string key)
        {
            if (_wsClients.ContainsKey(key))
            {
                _wsClients.Remove(key, out WSNotifyData removedItem);

                await removedItem.Client.CloseAsync(WebSocketCloseStatus.NormalClosure, "WebSocket connection has closed.", removedItem.ThreadCancellation.Token);
                removedItem.SocketTCS.TrySetResult(removedItem.Client);
                removedItem.ThreadCancellation.Cancel();
            }
        }

        public async Task<int> SendAsync(string key, byte[] data)
        {
            if (key is null)
            {
                int count = 0;
                foreach (var item in _wsClients.Where(c => c.Value.Client.State == WebSocketState.Open).Select(c => c.Value))
                {
                    await item.Client.SendAsync(data, WebSocketMessageType.Binary, endOfMessage: true, item.ThreadCancellation.Token);
                    count++;
                }
                return count;
            }

            if (_wsClients.ContainsKey(key) && _wsClients[key].Client.State == WebSocketState.Open)
            {
                await _wsClients[key].Client.SendAsync(data, WebSocketMessageType.Binary, endOfMessage: true, _wsClients[key].ThreadCancellation.Token);
                return 1;
            }

            return 0;
        }
        public async Task<int> SendAsync(string key, string data)
        {
            if (key is null)
            {
                int count = 0;
                foreach (var item in _wsClients.Where(c => c.Value.Client.State == WebSocketState.Open).Select(c => c.Value))
                {
                    await item.Client.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, endOfMessage: true, item.ThreadCancellation.Token);
                    count++;
                }
                return count;
            }

            if (_wsClients.ContainsKey(key) && _wsClients[key].Client.State == WebSocketState.Open)
            {
                await _wsClients[key].Client.SendAsync(Encoding.UTF8.GetBytes(data), WebSocketMessageType.Text, endOfMessage: true, _wsClients[key].ThreadCancellation.Token);
                return 1;
            }

            return 0;
        }
        public async Task<int> SendAsync(string key, Stream data, WSNotifyMessageType messageType = WSNotifyMessageType.Text)
        {
            if (key is null)
            {
                int count = 0;
                foreach (var item in _wsClients.Where(c => c.Value.Client.State == WebSocketState.Open).Select(c => c.Value))
                {
                    var buffer = new byte[4096];

                    long totalReadBytes = 0L;
                    int readBytes = await data.ReadAsync(buffer, 0, buffer.Length, item.ThreadCancellation.Token);
                    while (readBytes > 0)
                    {
                        bool endOfMessage = data.Length - totalReadBytes < buffer.Length;

                        await item.Client.SendAsync(
                            new ArraySegment<byte>(buffer, 0, readBytes),
                            messageType == WSNotifyMessageType.Text ? WebSocketMessageType.Text : WebSocketMessageType.Binary,
                            endOfMessage,
                            CancellationToken.None);

                        totalReadBytes += readBytes;
                        readBytes = await data.ReadAsync(buffer, 0, buffer.Length, item.ThreadCancellation.Token);
                    }
                    count++;
                }
                return count;
            }

            if (_wsClients.ContainsKey(key) && _wsClients[key].Client.State == WebSocketState.Open)
            {
                var buffer = new byte[4096];
                var item = _wsClients[key];

                long totalReadBytes = 0L;
                int readBytes = await data.ReadAsync(buffer, 0, buffer.Length, item.ThreadCancellation.Token);
                while (readBytes > 0)
                {
                    bool endOfMessage = data.Length - totalReadBytes < buffer.Length;

                    await item.Client.SendAsync(
                        new ArraySegment<byte>(buffer, 0, readBytes),
                        messageType == WSNotifyMessageType.Text ? WebSocketMessageType.Text : WebSocketMessageType.Binary,
                        endOfMessage,
                        CancellationToken.None);

                    totalReadBytes += readBytes;
                    readBytes = await data.ReadAsync(buffer, 0, buffer.Length, item.ThreadCancellation.Token);
                }
                return 1;
            }

            return 0;
        }

        private async void BindReceiver(WSNotifyData data)
        {
            var buffer = new byte[4096];

            WebSocketReceiveResult result = await data.Client.ReceiveAsync(buffer, data.ThreadCancellation.Token);
            while (!result.CloseStatus.HasValue)
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    while (!result.EndOfMessage)
                    {
                        await ms.WriteAsync(buffer, 0, result.Count, data.ThreadCancellation.Token);
                        result = await data.Client.ReceiveAsync(buffer, data.ThreadCancellation.Token);
                    }
                    await ms.WriteAsync(buffer, 0, result.Count, data.ThreadCancellation.Token);

                    if (OnReceive != null)
                    {
                        if (result.MessageType == WebSocketMessageType.Binary)
                            OnReceive(data.Key, Convert.ToBase64String(ms.ToArray()), WSNotifyMessageType.Binary);
                        else
                            OnReceive(data.Key, Encoding.UTF8.GetString(ms.ToArray()), WSNotifyMessageType.Text);

                    }
                }

                result = await data.Client.ReceiveAsync(buffer, data.ThreadCancellation.Token);
            }

            RemoveAsync(data.Key);
        }
    }
}
