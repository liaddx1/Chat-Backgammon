using Microsoft.AspNetCore.Mvc;

namespace signalRChatApiServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class DefaultController : ControllerBase
    {
        [HttpGet]
        public string Get() => "In Default Get Action";
    }
}
