
namespace WebSocketSample.Models
{
    public class PushedMessage
    {
        /// <summary>
        /// Key of target client
        /// </summary>
        /// <remarks>
        /// set it <see langword="null"/> to broadcast to everyone
        /// </remarks>
        public string Key { get; set; }

        public string Message { get; set; }
    }
}
