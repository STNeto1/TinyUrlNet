using KafkaFlow.Producers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Entities;
using UrlShortener.Utils;

namespace UrlShortener.Controllers;

[Route("api/shortener")]
[ApiController]
public class TinyUrlController : Controller
{
    private readonly DatabaseContext _context;
    private readonly IProducerAccessor _producerAccessor;

    public TinyUrlController(DatabaseContext context, IProducerAccessor producerAccessor)
    {
        _context = context;
        _producerAccessor = producerAccessor;
    }

    [HttpPost("create")]
    public async Task<ActionResult<TinyUrl>> PostCreateTinyUrl(CreateTinyUrl payload)
    {
        var producer = _producerAccessor.GetProducer("publish-task");

        var userId = ContextUserId.FromClaims(User);

        var newTinyUrl = new TinyUrl
        {
            LongUrl = payload.Url,
            ShortUrl = await Nanoid.Nanoid.GenerateAsync(size: 8),
            UserId = userId
        };
        _context.TinyUrls.Add(newTinyUrl);
        await _context.SaveChangesAsync();

        await producer.ProduceAsync("key", newTinyUrl);

        return CreatedAtAction(nameof(GetShowTinyUrl), new {id = newTinyUrl.Id}, newTinyUrl);
    }

    [HttpGet("search"), Authorize]
    public async Task<ActionResult<List<TinyUrl>>> GetSearchTinyUrls()
    {
        var usrId = ContextUserId.FromClaims(User);
        if (usrId is null)
        {
            return Problem("Unauthorized", null, 400);
        }

        var tinyUrls = await _context.TinyUrls.Where(t => t.UserId == usrId).ToListAsync();
        return Ok(tinyUrls);
    }

    [HttpGet("show/{id}"), Authorize]
    public async Task<ActionResult<TinyUrl>> GetShowTinyUrl(int id)
    {
        var usrId = ContextUserId.FromClaims(User);
        if (usrId is null)
        {
            return Problem("Unauthorized", null, 400);
        }

        var tinyUrl = await _context.TinyUrls.Where(t => t.UserId == usrId && t.Id == id).FirstOrDefaultAsync();
        if (tinyUrl is null)
        {
            return Problem("Resource not found", null, 404);
        }

        return Ok(tinyUrl);
    }

    [HttpDelete("delete/{id}"), Authorize]
    public async Task<ActionResult<TinyUrl>> DeleteTinyUrl(int id)
    {
        var usrId = ContextUserId.FromClaims(User);
        if (usrId is null)
        {
            return Problem("Unauthorized", null, 400);
        }

        var tinyUrl = await _context.TinyUrls.Where(t => t.UserId == usrId && t.Id == id).FirstOrDefaultAsync();
        if (tinyUrl is null)
        {
            return Problem("Resource not found", null, 404);
        }

        _context.TinyUrls.Remove(tinyUrl);
        await _context.SaveChangesAsync();


        return NoContent();
    }
}