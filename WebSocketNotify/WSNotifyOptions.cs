
namespace WebSocketNotify
{
    public class WSNotifyOptions
    {
        public const string CONFIG_NAME = "WSNotify";

        /// <summary>
        /// Route to WSNotify
        /// </summary>
        /// <remarks>(defaults to <c>/ws</c>)</remarks>
        public string Route { get; set; } = "/ws";

        /// <summary>
        /// No. of connection allowed per IP address
        /// </summary>
        /// <remarks>(defaults to <c>1</c> connection per IP)</remarks>
        public int ConnectionPerIP { get; set; } = 1;
    }
}
