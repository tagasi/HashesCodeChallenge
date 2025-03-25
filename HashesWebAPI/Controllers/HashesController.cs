namespace HashesWebApi.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;

[ApiController]
[Route("/")]
public class HashesController : ControllerBase
{
    private readonly IRabbitMqProducerService _rabbitMqService;
    private readonly IDbHelper _dbHelper;
    private readonly IMemoryCache _cache;

    public HashesController(IRabbitMqProducerService rabbitMqService, IDbHelper dbHelper, IMemoryCache cache)
    {
        _rabbitMqService = rabbitMqService;
        _dbHelper = dbHelper;
        _cache = cache;
    }

    [HttpPost("hashes")]
    public async Task<ActionResult> PostHashes()
    { 
      try {
        string[] hashes = HashManagement.ComputeSha1Hash();

        int ParallelThreads = 4;

        await Parallel.ForEachAsync(hashes, new ParallelOptions { MaxDegreeOfParallelism = ParallelThreads },
        async (hash, token) =>
        {
            await _rabbitMqService.PublishMessageAsync(hash);
        });

        return Ok(new
        {
            status = "success",
            result = hashes
        });
      }
      catch (Exception ex)
      {
          return StatusCode(500, "Internal Server Error: " + ex.Message);
      }
    }

    [HttpGet("hashes")]
    public async Task<ActionResult> GetHashes()
    { 
      try {
        var cacheKey = $"staticHashesResultset";

        if (!_cache.TryGetValue(cacheKey, out string result))
          {
            result = await _dbHelper.ExecuteStoredProcedureAsync("getHashes",new Dictionary<string, object>());;
            _cache.Set(cacheKey, result, new MemoryCacheEntryOptions
              {
                  AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(720) // Cache for 1 hour
              });
          }
        return Content(result, "application/json");
      }
      catch (Exception ex)
      {
          return StatusCode(500, "Internal Server Error: " + ex.Message);
      }
    }

    [HttpGet("hashes/invalidate")]
    public ActionResult<string> InvalidateStaticTranslationCache()
    {
      try {
        var cacheKey = $"staticHashesResultset";

        _cache.Remove(cacheKey);

        return Ok(new { Message = "Hashes Resultset cache invalidated successfully." });
      }
      catch (Exception ex)
      {
          return StatusCode(500, "Internal Server Error: " + ex.Message);
      }
    }

}