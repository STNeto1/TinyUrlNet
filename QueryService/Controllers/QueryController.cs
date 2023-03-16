using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Shared;
using StackExchange.Redis;

namespace QueryService.Controllers
{
    [Route("api/query")]
    [ApiController]
    public class QueryController : ControllerBase
    {
        private readonly IDatabase _redis;

        public QueryController(IConnectionMultiplexer multiplexer)
        {
            _redis = multiplexer.GetDatabase();
        }

        [HttpGet("show/{url}")]
        public async Task<IActionResult> GetShowTinyUrl(string url)
        {
            var result = await _redis.StringGetAsync(url);

            if (!result.HasValue)
            {
                return Problem("Value not found", null, 404);
            }

            var body = JsonSerializer.Deserialize<TinyUrlPayload>(result.ToString());
            return Redirect(body.LongUrl);
        }
    }
}