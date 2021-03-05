using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using WebSocketNotify;
using WebSocketSample.Models;

namespace WebSocketSample.Controllers
{
    [ApiController]
    public class WebSocketController : ControllerBase
    {
        private readonly WSNotifyHandler _wsNotifyHandler;

        public WebSocketController(WSNotifyHandler wsNotifyHandler)
        {
            _wsNotifyHandler = wsNotifyHandler;
        }

        [HttpPost("/ws/push")]
        public async Task<IActionResult> PushAsync(PushedMessage input)
        {
            int sentCount = await _wsNotifyHandler.SendAsync(input.Key, input.Message);
            return Ok(new { sentCount });
        }

        [HttpPost("/ws/broad")]
        public async Task<IActionResult> BroadcastAsync(PushedMessage input)
        {
            int sentCount = await _wsNotifyHandler.SendAsync(null, input.Message);
            return Ok(new { sentCount });
        }

        [HttpGet("/ws/list")]
        public IActionResult List()
        {
            return Ok(_wsNotifyHandler.List().ToDictionary(t => t.Key, q => q.Value.ToString()));
        }
    }
}
